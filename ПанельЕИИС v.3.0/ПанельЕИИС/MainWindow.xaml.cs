using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace ПанельЕИИС
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // 
            
            CreateStPanels();// создаем кнопки ЕИИС
        }
 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) // при закрытии 
        {
            OverwriteXML(); // для панели кнопок перезаписываем XML

            Environment.Exit(0); 
        }
         
        private void Grid2_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e) // клик по панели кнопок
        {
            Rolling_Click();
        }

        private void butRol_Click(object sender, RoutedEventArgs e)        // нажали кн. "Свернуть панель кнопок"
        {
            Rolling_Click();
        }

        private void butSet1_Click(object sender, RoutedEventArgs e)       // нажали кн. "Сохранить текущие настройки видимости"
        {
              OverwriteXML( );
        }

        private void butShowAllBut_Click(object sender, RoutedEventArgs e) // нажали кн. "Показать ВСЕ кнопки"
        {
            var stPan1 = StackPanel1.Children.OfType<StackPanel>().ToList();
            for (int j = 0; j < stPan1.Count; j++)
                stPan1[j].Visibility = Visibility.Visible;
        }

        private void butSet2_Click(object sender, RoutedEventArgs e)       // нажали кн. "Показать только нужные кнопки"    
        {
            var stPan1 = StackPanel1.Children.OfType<StackPanel>().ToList();
            for (int j = 0; j < stPan1.Count; j++)
            {
              var chBox = stPan1[j].Children.OfType<System.Windows.Controls.CheckBox>().ToList();
              stPan1[j].Visibility = (chBox[0].IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void but3_Click(object sender, RoutedEventArgs e)          // нажали кн. "Настройки"
        {
            Window1 WinSet = new Window1();
            WinSet.Show();
        }

        private void butSet4_Click(object sender, RoutedEventArgs e)       // нажали кн. "Перезагрузить конфигурацию"
        {
            var stPan1 = StackPanel1.Children.OfType<StackPanel>().ToList();
            for (int j = 0; j < stPan1.Count; j++)
                StackPanel1.Children.Remove(stPan1[j]);

            CreateStPanels();
        }

        private void Grid2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // клик по панели кнопок
        {
            System.Windows.Media.Animation.DoubleAnimation da = new System.Windows.Media.Animation.DoubleAnimation();
            //da.From = 0;

            if (Grid2.Width > 20)
            {
                da.To = 10;
                da.Duration = TimeSpan.FromSeconds(5);
                Grid2.BeginAnimation(Grid.WidthProperty, da);
                Grid2.Width = 10;
            }
            else
            {
                da.To = 400;
                da.Duration = TimeSpan.FromSeconds(5);
                Grid2.BeginAnimation(Grid.WidthProperty, da);
                Grid2.Width = 400;
            }
        }

        private void ButtonOnClick(object sender, EventArgs eventArgs)     // нажали кнопку ЕИИС
        {
            string fEXE = ""; // переменная под полный путь к EXE-шнику

            var ser = new XmlSerializer(typeof(ObservableCollection<ButtonEIIS>));// Инициализируеv новый экземпляр
            using (FileStream stream = new FileStream("PanButtons.xml", FileMode.Open)) //Инициализируем новый файловый поток 
            {                                                               
                ObservableCollection<ButtonEIIS> pathExe = (ObservableCollection<ButtonEIIS>)ser.Deserialize(stream);//заполняем перечислитель десериализованными данными из файлового потока в котором находится наш файл PanButtons.xml с настройками для кнопок

                int n = Convert.ToInt32(((StackPanel)sender).Name.Substring(((StackPanel)sender).Name.Length - 3));//номер кнопки получаем из имени кнопки, имя берем из отправителя события ( нумерация кнопок сверху вниз )

                fEXE = (pathExe.ElementAt(n)).PathEXE; // приготавливаем полный путь к .exe-шнику
            }

            if (File.Exists(fEXE)) // Определяем, существует ли заданный .exe-шник
                System.Diagnostics.Process.Start(fEXE); // запускаем .exe-шник назначеный на эту кнопку
            else
                System.Windows.MessageBox.Show($"{fEXE} файл не найден");//если по каким то причинам .exe-шник не найден
        }
         
        private void CreateStPanels() //ф              создать кнопку ЕИИС, вместо Button используется SteckPanel 
        {
            if (File.Exists("PanButtons.xml") == false) return;

            string fPNG = "";    // переменная под полный путь к рисунку который отображается в Image на кнопке
            string cBut = "";    // переменная под название на кнопке оно же и название подсистеиы
            bool   chB  = false; // переменная для флага видимости кнопки

            var ser = new XmlSerializer(typeof(ObservableCollection<ButtonEIIS>));// Инициализируеv новый экземпляр
             
            using (FileStream stream = new FileStream("PanButtons.xml", FileMode.Open, FileAccess.Read))//Инициализируем новый файловый поток
            {
                ObservableCollection<ButtonEIIS> but = (ObservableCollection<ButtonEIIS>)ser.Deserialize(stream);//заполняем перечислитель десериализованными данными из файлового потока в котором находится наш файл PanButtons.xml с настройками для кнопок

                StackPanel[] stpans  = new StackPanel[but.Count];
                System.Windows.Controls.CheckBox[] checkboxs = new System.Windows.Controls.CheckBox[but.Count];
                Image[] images       = new  Image[but.Count];
                TextBlock[] textbls  = new TextBlock[but.Count];

                for (int i = 0; i < but.Count; i++)
                {
                    fPNG = (but.ElementAt(i)).PathPNG;
                    cBut = (but.ElementAt(i)).ContBut;
                    chB  = (but.ElementAt(i)).VisBut ;
                     
                    checkboxs[i] = new System.Windows.Controls.CheckBox();

                    stpans[i] = new StackPanel();
                    stpans[i].Name = "stPan" + String.Format("{0:000}", i  );
                    stpans[i].Visibility = (chB == true) ? Visibility.Visible : Visibility.Collapsed ;
                    stpans[i].Orientation = System.Windows.Controls.Orientation.Horizontal;
                    //stpans[i].Width = 380;
                    //stpans[i].Height = 80;
                   stpans[i].Margin = new Thickness(0, 5, 0, 0);
                    stpans[i].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)((i + 1) * 80), (byte)((i + 1) * 30), (byte)((i + 1) * 5)));
                    stpans[i].MouseLeftButtonDown += ButtonOnClick;
                    var bind = new System.Windows.Data.Binding(); // биндинг
                    bind.Source = checkboxs[i];
                    bind.Path = new PropertyPath("IsChecked");
                    bind.Converter = new BooleanToVisibilityConverter();
                    BindingOperations.SetBinding(stpans[i], StackPanel.VisibilityProperty, bind); //
                    StackPanel1.Children.Add(stpans[i]);

                    checkboxs[i].Name = "chBox00" + i;
                    checkboxs[i].IsChecked = chB;
                    checkboxs[i].Margin = new Thickness(5, 5, 0, 0);
                    stpans[i].Children.Add(checkboxs[i]);

                    images[i] = new  Image(); // Width="{Binding ElementName=txtBoxTest, Path=Text}"
                    images[i].Name = "img00" + i;
                    var bindImg = new  Binding(); // биндинг
                    bindImg.Source = Slider1;
                    bindImg.Path = new PropertyPath("Value");
                    BindingOperations.SetBinding(images[i], Image.WidthProperty, bindImg); //
                    BindingOperations.SetBinding(images[i], Image.HeightProperty, bindImg); //
                      //   images[i].Width = 60;
                        //   images[i].Height = 60;
                    images[i].Margin = new Thickness(5, 10, 10, 10);
                    try{images[i].Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fPNG));}catch { }
                    stpans[i].Children.Add(images[i]);

                    textbls[i] = new TextBlock();
                    textbls[i].Name = "txtBl00" + i;
                    textbls[i].FontSize = 28;
                    textbls[i].Text = cBut;
                    textbls[i].TextWrapping = TextWrapping.Wrap;
                     textbls[i].Width = 300;
                    //textbls[i].Height = 80;
                    textbls[i].Margin = new Thickness(5, 5, 5, 5);
                    stpans[i].Children.Add(textbls[i]);
                }
            }
        }

        private void OverwriteXML()   //ф              переписываем XML              //
        {
            var serializer = new XmlSerializer(typeof(ObservableCollection<ButtonEIIS>));

            ObservableCollection<ButtonEIIS> butOfXML;
            try
            {
                using (FileStream stream = new FileStream("PanButtons.xml", FileMode.Open, FileAccess.Read))// читаем XML колекцию
                {
                    butOfXML = (ObservableCollection<ButtonEIIS>)serializer.Deserialize(stream);
                    var stPan1 = StackPanel1.Children.OfType<StackPanel>().ToList();
                    for (int i = 0; i < stPan1.Count; i++)
                    {
                        var chBox = stPan1[i].Children.OfType<System.Windows.Controls.CheckBox>().ToList();
                        butOfXML[i].VisBut = (chBox[0].IsChecked == true) ? true : false;
                    }
                }

                File.Delete("PanButtons.xml");// удаляем файл

                using (FileStream fs = new FileStream("PanButtons.xml", FileMode.OpenOrCreate))// создаем новый файл
                {
                    serializer.Serialize(fs, butOfXML); // сериализуем в него новые 
                }
            }
            catch { }
        }

        private void Rolling_Click()  //ф-я события  нажали кн. Roll-инг
        {
            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            //Height = screenHeight - 130;

            double screenWidth = SystemParameters.FullPrimaryScreenWidth;

            //Top = 100;
            //Left = 550;

            System.Windows.Media.Animation.DoubleAnimation da = new System.Windows.Media.Animation.DoubleAnimation();
            //da.From = 0;

            if (Grid2.Width > 20)
            {
                da.To = 10;
                da.Duration = TimeSpan.FromSeconds(1);
                Grid2.BeginAnimation(Grid.WidthProperty, da);

                Grid2.Width = 10;

                System.Windows.Media.TranslateTransform trans = new System.Windows.Media.TranslateTransform();
                //Grid4.RenderTransform = trans;
                System.Windows.Media.Animation.DoubleAnimation anim1 = new System.Windows.Media.Animation.DoubleAnimation(0, -400, TimeSpan.FromSeconds(1));
                trans.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim1);
                 
                Grid2.Width = 2*(screenWidth / 3);
            }
            else
            {
                da.To = 500;
                da.Duration = TimeSpan.FromSeconds(3);
                Grid2.BeginAnimation(Grid.WidthProperty, da);
                Grid2.Width = 500;
            }
        }
  
    }
  
}

 