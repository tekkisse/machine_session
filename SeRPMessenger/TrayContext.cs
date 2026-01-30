using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Windows.Forms;

namespace SeRPMessenger
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    namespace TrayRabbitApp
    {
        public class TrayContext : ApplicationContext
        {
            private readonly NotifyIcon _trayIcon;
            private readonly Control _uiInvoker;
            private IConnection _connection;
            private IChannel _channel;

            public TrayContext()
            {
                _uiInvoker = new Control();
                _uiInvoker.CreateControl(); // forces handle creation

                _trayIcon = new NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Information,
                    Text = "SeRP Notify",
                    Visible = true,
                    ContextMenuStrip = new ContextMenuStrip()
                };

                _trayIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);

                _ = StartRabbitMqAsync();
            }

            private async Task StartRabbitMqAsync()
            {
                var rabbitStuff = new RabbitMQStuff.CommonFunctions();
                var (channel, queue) = await rabbitStuff.ConfigureRabbit();

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    _uiInvoker.BeginInvoke(new Action(() =>
                    {
                        ShowPopup(message);
                    }));

                    await Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(queue, true, consumer);
                
            }

            private void ShowPopupOld(string message)
            {
                _trayIcon.ShowBalloonTip(
                    5000,
                    "Notification",
                    message,
                    ToolTipIcon.Info);
            }

            private void ShowPopup(string message)
            {
                var form = new Form
                {
                    Text = "Notification",
                    Width = 400,
                    Height = 200,
                    StartPosition = FormStartPosition.CenterScreen,
                    TopMost = true
                };

                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = message,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                };

                form.Controls.Add(label);
                form.Show();
            }

            private void Exit(object sender, EventArgs e)
            {
                _trayIcon.Visible = false;
                _channel?.CloseAsync();
                _connection?.CloseAsync();
                Application.Exit();
            }
        }
    }

}
