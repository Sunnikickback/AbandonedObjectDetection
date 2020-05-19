using System;

using System.Drawing;

using System.Windows.Forms;
using ClassLibrary;

using Emgu.CV;

using Emgu.CV.Structure;

using DirectShowLib;
using System.Collections.Generic;
using FastYolo;
using System.Linq;
using FastYolo.Model;
using System.IO;
using Telegram.Bot;
using MihaZupan;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Args;
using System.Drawing.Imaging;


namespace WindowsFormsApp
{
    public partial class CameraForm : Form
    {
        Random random = new Random();
        Data data = new Data();
        int code = 0;
        private VideoCapture capture = null;
        private DsDevice[] cams = null;
        private int selectedCameraID = -1;
        int frameNo = 0;
        YoloWrapper yolo = null;
        List<Item> people = new List<Item>();
        List<Item> objects = new List<Item>();
        List<PersonAndObject> tracking = new List<PersonAndObject>();
        ITelegramBotClient botClient;
        public CameraForm()
        {
            
            InitializeComponent();
            data = XMLManager.XMLReader("settings.xml");
            cams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < cams.Length; i++)
            {
                ComboBoxCamera.Items.Add(cams[i].Name);
            }
            yolo = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");
            var proxy = new HttpToSocks5Proxy("47.241.16.16", 1080);
            botClient = new TelegramBotClient("911904068:AAEJxycoiflY6SxJdu0wndtcTG7SdwVnX-0", proxy) { Timeout = TimeSpan.FromSeconds(10) };

        }
        
        
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ComboBoxCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCameraID = ComboBoxCamera.SelectedIndex;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            try
            {
                if(cams.Length == 0)
                {
                    throw new Exception("Нет доступных камер");
                }else if(data.Id == -1)
                {
                    throw new Exception("Telgram Id не указан, пожалуйста укажите его в настройках");
                }
                else if(ComboBoxCamera.SelectedItem == null)
                {
                    throw new Exception("Выберите камеру");
                }
                else if(capture != null)
                {
                    capture = new VideoCapture(selectedCameraID);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();
                }
                else
                {
                    capture = new VideoCapture(selectedCameraID);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();
                }
            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Ошибка!" ,MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void SendImage()
        {
            var output = "DetectedObject.png";
            using (FileStream fs = System.IO.File.OpenRead(output))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs);
                try
                {
                    await botClient.SendPhotoAsync(data.Id, inputOnlineFile);
                }
                catch (Exception)
                {
                    ;
                }
                
                

            }
        }
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            people = new List<Item>();
            objects = new List<Item>();
            frameNo++;
            Mat m = new Mat();
            capture.Retrieve(m);
            var bitmap = m.ToImage<Bgr, byte>().Bitmap;

            bitmap = new Bitmap(bitmap);
            Size size = bitmap.Size;
            List<YoloItem> items = yolo.Detect(FastYolo.ImageConverter.BitmapToColorData(bitmap)).ToList<YoloItem>();
            
            Font font = new Font("Arial", 10, FontStyle.Bold);
            Graphics graphics = Graphics.FromImage(bitmap);
            Pen pen = new Pen(System.Drawing.Color.Yellow, 2);
            SolidBrush brush = new SolidBrush(System.Drawing.Color.Yellow);
            foreach(var item in items)
            {
                Point rectPoint = new Point(item.X, item.Y);
                Size rectSize = new Size(item.Width, item.Height);
                Rectangle rect = new Rectangle(rectPoint, rectSize);
                try {
                    Item person = new Item(item, "person");
                    people.Add(person);
                }
                catch (ItemException) {
                    ;
                }
                try
                {
                    Item obj = new Item(item, "suitcase, handbag, backpack");
                    objects.Add(obj);
                }
                catch (ItemException)
                {
                    ;
                }
            }

            List<int> indexForDelete = new List<int>();
            for(int i = 0; i < tracking.Count; i++)
            {
                var pair = tracking[i];
                if (pair.UpdateCords(ref people, ref objects))
                {
                    if(pair.PersonIsAway == 15)
                    {
                        pen.Color = System.Drawing.Color.Red;
                        graphics.DrawRectangle(pen, pair.Item.Rect);
                        graphics.DrawString("CheckObject!!!", font, brush, pair.Item.Rect);
                        Bitmap bitmapForSave = new Bitmap(bitmap);
                        bitmapForSave.Save("DetectedObject.png", ImageFormat.Png);
                        SendImage();
                    }else if(pair.PersonIsAway > 15)
                    {
                        pen.Color = System.Drawing.Color.Red;
                        graphics.DrawRectangle(pen, pair.Item.Rect);
                        graphics.DrawString("CheckObject!!!", font, brush, pair.Item.Rect);
                    }
                }
                else
                {
                    indexForDelete.Add(i);
                }
            }
            foreach(var i in indexForDelete)
            {
                tracking.RemoveAt(i);
            }
            
            foreach (var obj in objects)
            {
                var track = obj.NewPair(ref people);
                if(track != null)
                {
                    tracking.Add(track);
                }
            }

            
            Point point = new Point(bitmap.Width - 100, 0);
            graphics.DrawString(frameNo + "", font, brush, point);
            bitmap = new Bitmap(bitmap, size);
            pictureBox.Image = bitmap;

            
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            try
            {
                if(capture != null)
                {
                    capture.Pause();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void никВТелеграммToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(capture != null)
            {
                capture.Pause();
            }
            botClient.OnMessage += bot_OnMessage;
            botClient.StartReceiving();
            code = random.Next(100000, 1000000);
            DialogResult result = MessageBox.Show($"Пожалуйста, отправьте уникальный шестизначный код боту @AbandonedObjectBot - {code}. \nПосле нажмите Ok", "Связь с ботом", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if(result == DialogResult.OK)
            {
                botClient.StopReceiving();
            }
        }
        private async void bot_OnMessage(object sender, MessageEventArgs e)
        {
            var text = e?.Message?.Text;
            if (text == null)
                return;
            if (text == (code + ""))
            {
                data.Id = e.Message.Chat.Id;
                XMLManager.XMLWriter(data, "settings.xml");
                try
                {
                    
                    await botClient.SendTextMessageAsync(data.Id, "Бот подключен, нажмите OK на компьютере, для продолжения работы");
                    botClient.StopReceiving();
                    
                }
                catch (Exception)
                {
                    ;
                }

            }
            else 
            { 
                try
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Неверный код доступа");
                }
                catch (Exception)
                {
                    ;
                }
            }
        }
    }
}
