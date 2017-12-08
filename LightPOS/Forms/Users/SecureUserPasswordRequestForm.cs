﻿using NickAc.LightPOS.Backend.Objects;
using NickAc.LightPOS.Backend.Translation;
using NickAc.LightPOS.Backend.Utils;
using NickAc.LightPOS.Frontend.Controls;
using NickAc.ModernUIDoneRight.Controls;
using NickAc.ModernUIDoneRight.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static NickAc.LightPOS.Frontend.Controls.UserPanel;

namespace NickAc.LightPOS.Frontend.Forms.Users
{
    class SecureUserPasswordRequestForm : ModernForm
    {
        public User User { get; set; }

        const int ControlPadding = 8;


        /// <summary>
        /// Called to signal to subscribers that login succeded
        /// </summary>
        public event EventHandler<UserEventArgs> LoginSucceded;
        protected virtual void OnLoginSucceded(User e)
        {
            EventHandler<UserEventArgs> eh = LoginSucceded;

            eh?.Invoke(this, new UserEventArgs(e));
        }

        public SecureUserPasswordRequestForm()
        {
            Sizable = false;
            TitlebarVisible = false;
            Opacity = 0;

        }

        public void SecureRequest(User usr)
        {
            User = usr;
            AddControls(this);
            ShowDialog();
        }


        public void Recenter(Control c, bool horizontal = true, bool vertical = true)
        {
            if (c == null) return;
            if (horizontal)
                c.Left = (c.Parent.ClientSize.Width - c.Width) / 2;
            if (vertical)
                c.Top = (c.Parent.ClientSize.Height - c.Height) / 2;
        }


        private void AddControls(SecureUserPasswordRequestForm form)
        {
            TranslationHelper translationHelper = new TranslationHelper();
            User usr = User;

            const float percentage = 0.25f;
            const float userNamePercentage = 0.25f;
            const float textBoxPercentage = 0.65f;

            KeyEventHandler escapeKeyHandler = (Object s, KeyEventArgs ee) => {
                if (ee.KeyCode == Keys.Escape && !ee.Control && !ee.Alt && !ee.Shift) {
                    this.InvokeIfRequired(form.Close);
                    ee.Handled = ee.SuppressKeyPress = true;
                }
            };
            Label mainLabel = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Text = translationHelper.GetTranslation("user_login_simple_title"),
                Font = new Font(base.Font.FontFamily, 16),
                Location = new Point(0, ControlPadding)
            };

            form.Controls.Add(mainLabel);
            Recenter(mainLabel, vertical: false);

            Label userNameLabel = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Text = usr.UserName,
                Font = new Font(base.Font.FontFamily, 12),
                Location = new Point(0, (int)(form.Height * userNamePercentage))
            };

            form.Controls.Add(userNameLabel);
            Recenter(userNameLabel, vertical: false);

            ModernButton btn = new ModernButton
            {
                Text = translationHelper.GetTranslation("user_login_okbutton"),
                Size = new Size((int)(form.Width * percentage), (int)(form.Height * percentage)),
            };
            btn.Location = new Point(0 /* Will be centered later */, form.Bottom - ControlPadding - btn.Height);

            Point point = new Point(0, (int)(form.Height * textBoxPercentage));
            point.Offset(0, -8);

            TextBoxEx textBox = new TextBoxEx
            {
                Font = userNameLabel.Font,
                UseSystemPasswordChar = true,
                Size = new Size((int)(form.Width * textBoxPercentage), 0 /* The textbox sizes automatically */),
            };
            point.Offset(0, -textBox.Height);
            textBox.Location = point;

            form.Controls.Add(textBox);
            Recenter(textBox, vertical: false);

            //Now we can add the button click
            btn.Click += (s, ee) => {
                if (!string.IsNullOrWhiteSpace(textBox.Text)) {
                    if (usr.CheckPassword(textBox.Text)) {
                        //Close our smal login-form
                        form.FormClosed += (sss, eee) => {
                            OnLoginSucceded(usr);
                        };
                        form.Close();
                    } else {
                        //Password doesn't work
                        //Clear the textbox
                        textBox.Clear();
                    }
                }
            };
            form.Controls.Add(btn);
            Recenter(btn, vertical: false);
            form.AcceptButton = btn;
            textBox.KeyUp += escapeKeyHandler;
            form.KeyUp += escapeKeyHandler;
            form.Load += (ss, ee) => {
                var anim = new Animation().WithLimit(10).WithAction((a) => form.InvokeIfRequired(() => form.Opacity += 0.1f)).WithDisposal(form);
                anim.Start();
            };
            bool canCloseForm = false;
            MethodInvoker reduceOpacity = () => form.Opacity -= 0.1f;

            form.FormClosing += (ss, ee) => {
                ee.Cancel = !canCloseForm;
                var anim = new Animation().WithLimit(10).WithAction((a) => {
                    if (Math.Abs(form.Opacity) < float.Epsilon) {
                        canCloseForm = true;
                        form.InvokeIfRequired(form.Dispose);
                        return;
                    }
                    form.InvokeIfRequired(reduceOpacity);
                }).WithDisposal(form);
                anim.Start();
            };
            translationHelper.Dispose();
        }
    }
}