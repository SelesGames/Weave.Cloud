using RedisDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Weave.GuiConsoleTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LinkedList<string> commandList = new LinkedList<string>();
        LinkedListNode<string> currentNode;

        public MainWindow()
        {
            InitializeComponent();
        }

        void commandLine_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessComand();
            else if (e.Key == Key.Up)
                ShowPreviousCommand();
            else if (e.Key == Key.Down)
                ShowNextCommand();
        }

        void ShowPreviousCommand()
        {
            if (currentNode == null || currentNode.Previous == null)
                return;

            currentNode = currentNode.Previous;
            commandLine.Text = currentNode.Value;
            commandLine.CaretIndex = int.MaxValue;
        }

        void ShowNextCommand()
        {
            if (currentNode == null || currentNode.Next == null)
                return;

            currentNode = currentNode.Next;
            commandLine.Text = currentNode.Value;
            commandLine.CaretIndex = int.MaxValue;
        }

        async void ProcessComand()
        {
            outputWindow.Text = "";
            var input = commandLine.Text;

            commandList.AddLast(input);
            currentNode = commandList.Last;

            var p = new Program();
            var output = await p.ProcessInput(input);
            outputWindow.Text = output;
        }
    }
}