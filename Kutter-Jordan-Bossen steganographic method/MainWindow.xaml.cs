using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Kutter_Jordan_Bossen_steganographic_method
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Запоминаем данные
        List<ValueTuple<int, int>> Key = new List<ValueTuple<int, int>>();
        Bitmap container;
        string currentFileName;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void buttonHidingData_Click(object sender, RoutedEventArgs e)
        {
            //Загружаем изображение
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string FilePath = openFileDialog.FileName;
                currentFileName = openFileDialog.SafeFileName.Split('.')[0];//запоминаем имя
                //Выводим оригинальное изображение
                BitmapImage inputBitmap = new BitmapImage();
                inputBitmap.BeginInit();
                inputBitmap.UriSource = new Uri(FilePath, UriKind.Absolute);
                inputBitmap.EndInit();

                imageInput.Source = inputBitmap;

                imageOutput.Visibility = Visibility.Visible;
                textBoxOutput.Visibility = Visibility.Hidden;
                //Выводим измененное изображение
                imageOutput.Source = BitmapToBitmapImageBMP(KDBHidingData(FilePath, textBoxMessage.Text, Convert.ToInt32(textBoxRepeatCount.Text)));

                buttonSave.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// Скрывает сообщение в изображении.
        /// <para>Для сокрытия необходимо указать: путь к изображению, текст сообщения,
        /// количество встраиваний текста в изображение</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="NUM_OF_REPEATS"></param>
        /// <returns>Измененное изображение Bitmap.</returns>
        Bitmap KDBHidingData(string path, string text, int NUM_OF_REPEATS)
        {
            //NUM_OF_REPEATS - количество встраиваниий
            //Множим сообщение
            string ms = text;
            for (int i = 1; i < NUM_OF_REPEATS; i++)
            {
                text += ms;
            }
            //Загружаем изображение
            Bitmap img = new Bitmap(path);
            int imgWidth = img.Width;
            int imgHeight = img.Height;
            //Получаем каналы 
            var channels = BitmapToByteRgb(img);
            //Переводим текст в битовый массив
            BitArray bitsMessage = StringToBit(text);
            //Проверяем вместимость сообщения в контейнер с учетом повторений
            if (bitsMessage.Count > ((imgWidth / 4 - 1) * (imgHeight / 4 - 1)))
            {
                MessageBox.Show("Изображение слишком мало для заданного текста");
                Bitmap bitmap = new Bitmap(imgWidth, imgHeight);
                return bitmap;
            }
            //Генерируем координаты
            var Pixels = Generator(bitsMessage.Count, imgHeight, imgWidth, 3);
            //Встраиваем биты
            for (int i = 0; i < bitsMessage.Count; i++)
            {
                int red = channels[Pixels[i].Item1, Pixels[i].Item2, 0];
                int green = channels[Pixels[i].Item1, Pixels[i].Item2, 1];
                int blue = channels[Pixels[i].Item1, Pixels[i].Item2, 2];
                //Высчитываем яркость
                int L = (int)(0.29890 * red + 0.58662 * green + 0.11448 * blue);
                double energy = 0.25;
                int modifiedBlue;
                if (bitsMessage[i] == true)//1
                {
                    modifiedBlue = (int)(blue + (energy * L));
                }
                else //0
                {
                    modifiedBlue = (int)(blue - (energy * L));
                }
                //проверка выхода за границы
                if (modifiedBlue > 255)
                {
                    modifiedBlue = 255;
                }
                if (modifiedBlue < 0)
                {
                    modifiedBlue = 0;
                }
                //модифицируем изображение
                //Обращаю внимание, что в SetPixel сначала идет ширина, а потом высота
                img.SetPixel(Pixels[i].Item2, Pixels[i].Item1, System.Drawing.Color.FromArgb(red, green, (byte)modifiedBlue));
            }
            //Запоминаем данные для возможности сохранения
            container = img;
            Key = Pixels;
            return img;
        }
        /// <summary>
        /// Генерирует набор координат заданного размера. Размер текста должен учитыватья заранее.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="sigma"></param>
        /// <returns> Набор List ValueTuple int, int координат. 
        /// </returns>
        List<ValueTuple<int, int>> Generator(int count, int height, int width, int sigma)
        {
            // Использование списка вместо массива в два раза быстрее, при использовании Contains
            List<ValueTuple<int, int>> Pixels = new List<ValueTuple<int, int>>();
            var rand = new Random();
            for (int i = 0; i < count; i++)
            {
                int x = rand.Next(0, height - sigma * 2) + sigma;
                int y = rand.Next(width - sigma * 2) + sigma;
                while (Pixels.Contains(new ValueTuple<int, int>(x, y)))
                {
                    x = rand.Next(height - sigma * 2) + sigma;
                    y = rand.Next(width - sigma * 2) + sigma;
                }
                Pixels.Add(new ValueTuple<int, int>(x, y));
            }
            return Pixels;
        }
        /// <summary>
        /// Преоббразует изображение Bitmap в  BitmapImage.
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns>Изображение Bitmap.</returns>
        public BitmapImage BitmapToBitmapImageBMP(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);//выбираем формат изображения
            BitmapImage bmpI = new BitmapImage();
            bmpI.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bmpI.StreamSource = ms;
            bmpI.EndInit();
            return bmpI;
        }
        /// <summary>
        /// Преобразует строку в массив битов BitArray.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Массив битов BitArray.</returns>
        BitArray StringToBit(string str)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); //Для .NET
            byte[] bytes = Encoding.GetEncoding("windows-1251").GetBytes(str);
            BitArray bits = new BitArray(bytes);
            return bits;
        }
        /// <summary>
        /// Преобразует массив битов BitArray в строку string.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns>Строка srting.</returns>
        string BitToString(BitArray bits)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); //Для .NET
            int byte_length = Convert.ToInt32(Math.Ceiling(bits.Count / 8.0));
            byte[] bytes = new byte[byte_length];
            bits.CopyTo(bytes, 0);
            return Encoding.GetEncoding("windows-1251").GetString(bytes);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e) //Сохранить
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = currentFileName + "_contained"; // Default file name
            dlg.Filter = "Все файлы(*.*) | *.*";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                //Сохраняем изображение
                container.Save(filename + ".bmp", ImageFormat.Bmp);
                //Сохраняем ключ
                using (StreamWriter writer = new StreamWriter(filename + ".txt", false, System.Text.Encoding.Default))
                {
                    writer.WriteLine(textBoxRepeatCount.Text);//сохраняем кол-во повторов(встраиваний)
                    foreach (var coordinate in Key)//сохраняем координаты
                    {
                        writer.WriteLine(coordinate.Item1 + "," + coordinate.Item2);
                    }
                }
            }
        }

        private void buttonExtractData_Click(object sender, RoutedEventArgs e)//Извлечь
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Image Files(*.BMP;*.JPG;)|*.BMP;*.JPG;|All files (*.*)|*.*";
            openFileDialog.Filter = "Image Files(*.BMP)|*.BMP|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string FilePath = openFileDialog.FileName;
                currentFileName = openFileDialog.SafeFileName.Split('.')[0];

                BitmapImage inputBitmap = new BitmapImage();
                inputBitmap.BeginInit();
                inputBitmap.UriSource = new Uri(FilePath, UriKind.Absolute);
                inputBitmap.EndInit();

                textBoxOutput.Visibility = Visibility.Visible;
                imageOutput.Visibility = Visibility.Hidden;
                buttonSave.Visibility = Visibility.Hidden;

                imageInput.Source = inputBitmap;

                string keyPath = FilePath.Replace(".bmp", ".txt");
                //Извлекаем сообщение
                textBoxOutput.Text = KDBExtractingData(FilePath, keyPath);
            }
        }
        /// <summary>
        /// Извлекает сообщение из контейнера. Производит усреднение цвета.
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="keyPath"></param>
        /// <returns>Текст сообщения и текст, полученный в результате усреднения разницы всех оценок бита.</returns>
        string KDBExtractingData(string imagePath, string keyPath)
        {
            //загружаем изображение
            Bitmap img = new Bitmap(imagePath);
            //получаем каналы 
            var channels = BitmapToByteRgb(img);
            //получаем координаты
            List<ValueTuple<int, int>> Coordinates = new List<ValueTuple<int, int>>();
            int NUM_OF_REPEATS;
            using (StreamReader reader = new StreamReader(keyPath))
            {
                string? line;
                NUM_OF_REPEATS = Convert.ToInt32(reader.ReadLine());
                while ((line = reader.ReadLine()) != null)
                {
                    int x = Convert.ToInt32(line.Split(',')[0]);
                    int y = Convert.ToInt32(line.Split(',')[1]);

                    Coordinates.Add(new ValueTuple<int, int>(x, y));
                }
            }

            int sigma = 3;//количество пикселей сверху (снизу, слева, справа) от оцениваемого пикселя
            List<bool> bits = new List<bool>();//биты текста
            List<int> delta = new List<int>();//список значений цвета, используется в усреднении
            foreach (var pixel in Coordinates)
            {
                int sum = 0;//сумма креста
                //справа
                for (int i = 1; i <= sigma; i++)
                {
                    sum += channels[pixel.Item1 + i, pixel.Item2, 2];
                }
                //слева
                for (int i = 1; i <= sigma; i++)
                {
                    sum += channels[pixel.Item1 - i, pixel.Item2, 2];
                }
                //сверху
                for (int i = 1; i <= sigma; i++)
                {
                    sum += channels[pixel.Item1, pixel.Item2 + i, 2];
                }
                //снизу
                for (int i = 1; i <= sigma; i++)
                {
                    sum += channels[pixel.Item1, pixel.Item2 - i, 2];
                }
                int blue = channels[pixel.Item1, pixel.Item2, 2];//текущее значение цвета
                int B = sum / (4 * sigma);//предсказанное значение
                if (blue > B)
                {
                    bits.Add(true);//1
                }
                else
                {
                    bits.Add(false);//0
                }
                delta.Add(blue - B);//запоминаем значение цвета
            }
            //Усредрение разницы всех оценок бита s = SUM(blue - B)/NUM_OF_REPEATS

            List<bool> bits2 = new List<bool>();//биты текста с усреднением
            var Variants = delta.Chunk(delta.Count / NUM_OF_REPEATS);//делим массив на количество повторений
            int count = 0;
            while (count != delta.Count / NUM_OF_REPEATS)
            {
                int sum = 0;
                foreach (var Variant in Variants)
                {
                    sum += Variant[count];
                }
                int s = sum / NUM_OF_REPEATS;//Усредрение разницы всех оценок бита
                if (s > 0)
                {
                    bits2.Add(true);
                }
                else bits2.Add(false);
                count++;
            }

            BitArray bitsMessage = new BitArray(bits.ToArray());
            BitArray bitsDelta = new BitArray(bits2.ToArray());
            return BitToString(bitsMessage) + "\r\n\r\nУсреднение: " + BitToString(bitsDelta);
        }
        /// <summary>
        /// Извлекает яркость пикселей по 3м каналам 
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns>
        /// 3-х мерный массив числовых значений яркости пикселей на манер матрицы numpy, где первое значение означает количество строк, а второе - количество столбцов.
        /// <para> Формат: [Высота, Ширина, Канал].</para>
        /// <para> "Канал" принимает значения:</para>
        /// <para>0 - Красный</para>
        /// <para>1 - Зеленый</para>
        /// <para>2 - Синий</para>
        /// </returns>
        public unsafe static byte[,,] BitmapToByteRgb(Bitmap bmp)
        {
            int width = bmp.Width,
                height = bmp.Height;
            byte[,,] res = new byte[height, width, 3];
            BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                for (int h = 0; h < height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < width; w++)
                    {
                        res[h, w, 2] = *(curpos++);
                        res[h, w, 1] = *(curpos++);
                        res[h, w, 0] = *(curpos++);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return res;
        }
    }
}
