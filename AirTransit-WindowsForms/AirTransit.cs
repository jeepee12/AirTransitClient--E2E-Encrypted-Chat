﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Message = AirTransit_Core.Models.Message;
using System.Threading.Tasks;
using AirTransit_Core.Models;
using AirTransit_Core;
using AirTransit_Core.Repositories;
using AirTransit_Core.Services;

namespace AirTransit_WindowsForms
{
    public partial class AirTransit : Form
    {
        private delegate void StringArgReturningVoidDelegate(string text);
        private delegate void MessageArgReturningVoidDelegate(Message text);

        private string phoneNumber;
        private ConcurrentBag<Contact> contacts;
        private BlockingCollection<Message> newMessageIds;
        private CoreServices Core;
        private IContactRepository ContactRepo;
        private IMessageRepository MessageRepo;
        private IMessageService MessageService;
        private Color UserColor = Color.DarkRed;
        private Color ContactColor = Color.DarkBlue;
        private bool WasUser;
        private Contact selectedContact;

        public AirTransit()
        {
            Core = new CoreServices();
            InitializeComponent();
        }

        private void AirTransit_Load(object sender, EventArgs e)
        {
            DialogResult LoginCompleted;
            using (FormLogin login = new FormLogin())
            {
                LoginCompleted = login.ShowDialog();
                PhoneNumber = login.PhoneNumber;
            }
            if (LoginCompleted == DialogResult.Abort)
            {
                MessageBox.Show("Login aborted. Closing...");
                Close();
            }
            else
            {
                if (Core.Init(phoneNumber))
                {
                    ContactRepo = Core.ContactRepository;
                    MessageRepo = Core.MessageRepository;
                    MessageService = Core.MessageService;
                    Contacts = new ConcurrentBag<Contact>(ContactRepo.GetContacts().ToList());
                    newMessageIds = Core.GetBlockingCollection();

                    // Based on : https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/blockingcollection-overview
                    Task.Run(() =>
                    {
                        while (!newMessageIds.IsCompleted)
                        {

                            Message message = null;
                            // Blocks if number.Count == 0
                            // IOE means that Take() was called on a completed collection.
                            // Some other thread can call CompleteAdding after we pass the
                            // IsCompleted check but before we call Take. 
                            // In this example, we can simply catch the exception since the 
                            // loop will break on the next iteration.
                            try
                            {
                                message = newMessageIds.Take();
                            }
                            catch (InvalidOperationException) { }

                            if (message != null)
                            {
                                ProcessNewMessage(message);
                            }
                        }
                    });
                    if (ListContacts.SelectedItem == null && ListContacts.Items.Count > 0)
                    {
                        ListContacts.SelectedIndex = 0;
                    }
                }
                else
                {
                    MessageBox.Show("An error as occur during initialization. Closing.");
                    Close();
                }
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (selectedContact != null)
            {
                if (string.IsNullOrWhiteSpace(TxtInput.Text))
                {
                    MessageBox.Show("The message must not be empty.");
                }
                else
                {
                    MessageService.SendMessage(selectedContact, TxtInput.Text);
                    PrintMessage(MessageRepo.GetLastMessage(selectedContact));
                    TxtInput.ResetText();
                }
            }
            else
                MessageBox.Show("Plz select a contact before sending a message.");
        }

        private void BtnContact_Click(object sender, EventArgs e)
        {
            NewContact newContact = new NewContact();
            if (newContact.ShowDialog() == DialogResult.OK)
            {
                ContactRepo.AddContact(new Contact(newContact.PhoneNumber, newContact.ContactName));
            }

            // Refresh contacts list
            Contacts = new ConcurrentBag<Contact>(ContactRepo.GetContacts().ToList());
        }

        private void AirTransit_FormClosing(object sender, FormClosingEventArgs e)
        {
            newMessageIds?.CompleteAdding();
        }

        private void ShowCurrentContactConvo()
        {
            if (selectedContact != null)
                ShowConvo(selectedContact);
        }

        private void ShowConvo(Contact contact)
        {
            Txtconversation.ResetText();
            MessageRepo?.GetMessages(contact)?.ToList().ForEach(PrintMessage);
        }

        private void PrintMessage(Message message)
        {
            if (message == null) return;
            if (Txtconversation.InvokeRequired)
            {
                MessageArgReturningVoidDelegate d = PrintMessage;
                Invoke(d, message);
            }
            else
            {
                bool currentlyUser = message.DestinationPhoneNumber != PhoneNumber;
                Txtconversation.SelectionColor = currentlyUser ? UserColor : ContactColor;
                if (WasUser != currentlyUser || Txtconversation.TextLength == 0)
                {
                    AppendTextSafely($"{message.Sender.Name} :");
                    WasUser = currentlyUser;
                }

                AppendTextSafely(message.Content + Environment.NewLine);
            }
        }

        private void AppendTextSafely(string message)
        {
            // Based on msdn information: https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls
            if (Txtconversation.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = AppendTextSafely;
                Invoke(d, message);
            }
            else
            {
                Txtconversation.AppendText(message);
            }
        }

        private void ProcessNewMessage(Message newMessage)
        {

            Contact senderContact = newMessage.Sender;
            if (senderContact.Id == selectedContact.Id)
            {
                PrintMessage(newMessage);
            }
            else if (!Contacts.Contains(senderContact))
            {
                // Adds the new contact to the contacts list
                Contacts.Add(senderContact);
            }
        }

        private string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                phoneNumber = value;
                TxtConnectedPhone.Text = phoneNumber;
            }
        }

        private ConcurrentBag<Contact> Contacts
        {
            get => contacts;
            set
            {
                contacts = value;
                List<Contact> inList = value.ToList();
                int clientContact = inList.FindIndex(c => c.PhoneNumber == PhoneNumber);
                // FindIndex return -1 if nothing is found
                if (clientContact != -1)
                {
                    inList.RemoveAt(clientContact);
                }
                ListContacts.DataSource = inList;
            }
        }

        private void ListContacts_MouseClick(object sender, MouseEventArgs e)
        {
            ShowCurrentContactConvo();
        }

        private void ListContacts_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (selectedContact != null)
            {
                NewContact newContact = new NewContact(selectedContact.PhoneNumber, selectedContact.Name);
                if (newContact.ShowDialog() == DialogResult.OK)
                {
                    selectedContact.Name = newContact.ContactName;
                    ContactRepo.UpdateContact(selectedContact);
                }
                Contacts = new ConcurrentBag<Contact>(ContactRepo.GetContacts().ToList());
            }
        }

        private void ListContacts_SelectedValueChanged(object sender, EventArgs e)
        {
            selectedContact = ListContacts.SelectedItem as Contact;
            ShowCurrentContactConvo();
        }
    }
}