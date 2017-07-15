//-----------------------------------------------------------------------
// <copyright file="MailUtil.cs" company="Sitecore Corporation">
//     Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>Defines the MailUtil class.</summary>
//-----------------------------------------------------------------------
// Copyright 2016 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the 
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// either express or implied. See the License for the specific language governing permissions 
// and limitations under the License.
// -------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Mail;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Managers
{
    public class MailManager : IManager
    {
        public bool SendMail(string templateName, string toEmail, params object[] bodyParameters)
        {
            Assert.ArgumentNotNull(templateName, nameof(templateName));
            Assert.ArgumentNotNull(toEmail, nameof(toEmail));
            Assert.ArgumentNotNull(bodyParameters, nameof(bodyParameters));

            var mailTemplates = GetMailTemplatesRoot();
            if (mailTemplates == null)
            {
                Log.Error($"Invalid mailTemplatesRoot on the site '{Sitecore.Context.Site.Name}'", this);
                return false;
            }

            var mailTemplate = mailTemplates.Children[templateName];
            if (mailTemplate == null || mailTemplate.IsDerived(Templates.MailTemplate.ID))
            {
                Log.Error($"Could not find email template {templateName}", this);
                return false;
            }

            var subject = mailTemplate.Fields[Templates.MailTemplate.Fields.Subject].Value;
            var body = string.Format(mailTemplate.Fields[Templates.MailTemplate.Fields.Body].Value, bodyParameters);
            var from = mailTemplate.Fields[Templates.MailTemplate.Fields.From].Value;
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(subject))
            {
                Log.Error($"Sender is not set on email template {templateName}", this);
                return false;
            }

            return SendMail(toEmail, from, subject, body, string.Empty);
        }

        private Item GetMailTemplatesRoot()
        {
            var mailTemplatesRoot = Context.Site.Properties["mailTemplatesRoot"];
            if (mailTemplatesRoot == null)
            {
                return null;
            }
            var item = Context.Site.Database.GetItem(mailTemplatesRoot);
            return item;
        }

        private bool SendMail(string toEmail, string fromEmail, string subject, string body, string attachmentFileName)
        {
            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Body = body,
                Subject = subject,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            if (attachmentFileName != null && File.Exists(attachmentFileName))
            {
                var attachment = new Attachment(attachmentFileName);
                message.Attachments.Add(attachment);
            }

            try
            {
                MainUtil.SendMail(message);

                Log.Info($"Sent message to {message.To} wth subject {message.Subject}", "SendMailFromTemplate");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Could not send mail message {message.Subject} to {message.To}", e, "SendMailFromTemplate");
                return false;
            }
        }
    }
}