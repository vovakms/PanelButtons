using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ПанельЕИИС
{
    public partial class Window1 : Window
    {
        ObservableCollection<ButtonEIIS> collection = null;

        public Window1()
        {
            InitializeComponent();

            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            Height = screenHeight - 130; // высота

            double screenWidth = SystemParameters.FullPrimaryScreenWidth;
            Width = screenWidth - 400; // ширина окна

            Top = 130;
            Left = 400;

        }
         
        private void butOpen_Click(object sender, RoutedEventArgs e)// нажали кн. Выбор каталога
        {
            txtBlockErr.Visibility = Visibility.Collapsed ;// прячем надпись
            txtBlockSave.Visibility = Visibility.Collapsed;

            try
            {
                collection.Clear();// очищаем колекцию тем самым очищается ДатаГрид
            }                      // если ДатаГрид был загружен из XML то будет ошибка  
            catch
            {                // поетому чистим через ДатаГридИсточник
                collection = null;
                DataGrid1.ItemsSource = null;
            }

            try
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                DialogResult result = folderBrowser.ShowDialog(); // выбиираем каталог
                if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                    txtPath.Text = folderBrowser.SelectedPath.ToString();
                AddButtons2DataGrid(folderBrowser.SelectedPath.ToString()); // вызываем ф. 
            }
            catch { }
        }

        private void AddButtons2DataGrid(string pathEIIS) // Заполняем коллекцию и отправляем ее в ДатаГрид
        {
            collection = null; // ?

            DirectoryInfo dirEIIS = new DirectoryInfo(pathEIIS);//получаем атрибуты выбранного каталога

            if (collection == null)// если наша коллекция пусто
            {
                collection = new ObservableCollection<ButtonEIIS>();
                DataGrid1.ItemsSource = collection;
            }

            foreach (var dir in dirEIIS.GetDirectories())// перебираем папки из корневого каталога
            {
                collection.Add(   // добавляем в коллекцию по одной записи
                new ButtonEIIS()
                {
                    VisBut = true,
                    ContBut = dir.Name.ToString(),
                    PathPNG = GetPathImg(dir.FullName.ToString()),
                    PathEXE = GetPathR(dir.FullName.ToString(), "*.exe"),
                    NameSubEIIS = collection.Count.ToString()
                }
                );
            }
        }

        private string GetPathImg(string dirSub )// ищем файлы изображений 
        {
            FileInfo[] files = null ;

            DirectoryInfo dinfo = new DirectoryInfo(dirSub);//получаем свединия указанной папки

            string[] extensions = new[] { ".ico", ".jpg", ".bmp", ".png"  };

            files = dinfo.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();

            if (files.Count()  != 0) return files[0].FullName.ToString();

            foreach (var fD in dinfo.GetDirectories() )
            {
                try
                {
                    files = fD.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                }
                catch { }
                if (files.Count() != 0) return files[0].FullName.ToString();
            }
             
            return "0" ; // если нет файла с указанным расширением возращаем 0
        }

        private string ExtractIcon(string nameEXE, string pathEXE, string pathDir )// извлекаем иконки из EXE-шников 
        {
            //System.Drawing.Icon.ExtractAssociatedIcon(PathEXE);

             using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(pathEXE))
             using (var file = File.Create(pathDir + @"\"+ nameEXE + ".ico"))
               icon.Save(file);
             return "0"; 
        }

        private string GetPathR(string dirSub, string r)// ищем файлы в указанной папке и заданным расширением 
        {
            //string PathEXE = "";

            DirectoryInfo dir = new DirectoryInfo(dirSub);//получаем свединия указанной папки
            foreach (var fR in dir.GetFiles(r))//перебираем файлы в этой папке с указанным расширением
            {
                //ExtractIcon(fR.FullName, dirSub);
                return fR.FullName;//возращаем первый попавшийся
            }

            return "0"; // если нет файла с указанным расширением возращаем путь к этой папке
        }

        private void butOfXML_Click(object sender, RoutedEventArgs e) // нажали кн. "Показать ТЕКУЩУЮ КОНФИГУРАЦИЮ"
        {
            txtBlockSave.Visibility = Visibility.Collapsed; // прячем надпись 

            try
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<ButtonEIIS>));

                using (FileStream stream = new FileStream("PanButtons.xml", FileMode.Open))
                {
                    ObservableCollection<ButtonEIIS> butOfXML = (ObservableCollection<ButtonEIIS>)serializer.Deserialize(stream);
                    DataGrid1.ItemsSource = butOfXML; // в ДатаГрид загружаем колекцию кнопок
                }
            }
            catch {
                txtBlockErr.Visibility = Visibility.Visible; // показываем надпись
            }
        }

        private void butSave_Click(object sender, RoutedEventArgs e) // нажали кн. "Сохранить"
        {
            File.Delete("PanButtons.xml");// удаляем файл

            var serializer = new XmlSerializer(typeof(ObservableCollection<ButtonEIIS>));

            using (FileStream fs = new FileStream("PanButtons.xml", FileMode.OpenOrCreate))
            {
                serializer.Serialize(fs, DataGrid1.ItemsSource as ObservableCollection<ButtonEIIS>);
            }

            txtBlockSave.Visibility = Visibility.Visible;// показываем надпись об удачном сохранении

            try
            {
                collection.Clear();// очищаем колекцию тем самым очищается ДатаГрид
            }                      // если ДатаГрид был загружен из XML то будет ошибка  
            catch {                // поетому чистим через ДатаГридИсточник
                collection = null;
                DataGrid1.ItemsSource = null;
            }
        }

        private void DataGrid1_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)// загрузка новой строки
        {
            e.Row.Header = ((e.Row.GetIndex()) + 1).ToString();
        }

        private void butClose_Click(object sender, RoutedEventArgs e) // нажали кн. Закрыть окно
        {
            Close();
        }

        private void butMax_Click(object sender, RoutedEventArgs e) // нажали кн. Максимизировать окно
        {
            if (WindowSet.WindowState == WindowState.Normal)
                WindowSet.WindowState = WindowState.Maximized;
            else
                WindowSet.WindowState = WindowState.Normal;
        }

        private void butMin_Click(object sender, RoutedEventArgs e) // нажали кн. Свернуть окно
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void WindowSet_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) // перемещение формы лев.кн.мыш.
        {
            DragMove(); //перемещаем форму
        }

        private void butParsIcons_Click(object sender, RoutedEventArgs e) // готовим иконки 
        {
            string pathDir = txtPath.Text.ToString();

            DirectoryInfo dirEIIS = new DirectoryInfo(pathDir);//получаем атрибуты выбранного каталога

            foreach (var dir in dirEIIS.GetDirectories())// перебираем папки из корневого каталога
            {
                // DirectoryInfo dir = new DirectoryInfo(dirSub);//получаем свединия указанной папки
                foreach (var fR in dir.GetFiles("*.exe"))//перебираем файлы в этой папке с указанным расширением
                {
                    
                    ExtractIcon(fR.Name.ToString(), fR.FullName, fR.Directory.ToString() ); // вызываем ф. извлечения иконок из EXE-шников
                }
            }
        }

        private void butUpd_Click(object sender, RoutedEventArgs e) // нажали кн. Обновить
        {
            txtBlockErr.Visibility = Visibility.Collapsed;// Прячем надпись
            txtBlockSave.Visibility = Visibility.Collapsed;

            AddButtons2DataGrid(txtPath.Text.ToString()); // вызываем ф.
        }
    }

    [Serializable]
    public class ButtonEIIS
    {
        public bool VisBut { get; set; }
        public string ContBut { get; set; }
        public string PathPNG { get; set; }
        public string PathEXE { get; set; }
        public string NameSubEIIS { get; set; }

    }

}
