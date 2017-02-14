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
using System.Globalization;
using System.IO;
using System.Net.Mail;
using Sitecore.Diagnostics;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;

namespace Sitecore.Foundation.Commerce.Util
{
    public class MailUtil
    {
        private string _mailAttachmentFileName = string.Empty;
        private string _mailBody = string.Empty;
        private string _mailFrom = string.Empty;
        private string _mailSubject = string.Empty;
        private string _mailTo = string.Empty;

        public virtual bool SendMail([NotNull] MailTemplate mailTemplate)
        {
            Assert.ArgumentNotNull(mailTemplate, nameof(mailTemplate));
            Assert.ArgumentNotNull(mailTemplate.ToEmail, nameof(mailTemplate.ToEmail));
            Assert.ArgumentNotNull(mailTemplate.FromEmail, nameof(mailTemplate.FromEmail));

            return SendMail(mailTemplate.ToEmail, mailTemplate.FromEmail, mailTemplate.Subject, mailTemplate.Body, string.Empty);
        }

        public virtual bool SendMail([NotNull] string templateName, [NotNull] string toEmail, [NotNull] string fromEmail, [NotNull] object subjectParameters, [NotNull] object[] bodyParameters)
        {
            Assert.ArgumentNotNull(templateName, nameof(templateName));
            Assert.ArgumentNotNull(toEmail, nameof(toEmail));
            Assert.ArgumentNotNull(fromEmail, nameof(fromEmail));
            Assert.ArgumentNotNull(subjectParameters, nameof(subjectParameters));
            Assert.ArgumentNotNull(bodyParameters, nameof(bodyParameters));

            var mailTemplates = StorefrontManager.CurrentStorefront.GlobalItem.Children[StorefrontConstants.KnowItemNames.Mails];
            if (mailTemplates == null)
            {
                return false;
            }

            var mailTemplate = mailTemplates.Children[templateName];
            if (mailTemplate == null)
            {
                Log.Error($"Could not find email template {templateName}", this);
                return false;
            }

            var subjectField = mailTemplate.Fields[StorefrontConstants.KnownFieldNames.Subject];
            if (subjectField == null)
            {
                Log.Error($"Could not find email subject for template {templateName}", this);
                return false;
            }

            var bodyField = mailTemplate.Fields[StorefrontConstants.KnownFieldNames.Body];
            if (bodyField == null)
            {
                Log.Error($"Could not find email body message for template {templateName}", this);
                return false;
            }

            var subject = string.Format(CultureInfo.InvariantCulture, subjectField.ToString(), subjectParameters);
            var body = string.Format(CultureInfo.InvariantCulture, bodyField.ToString(), bodyParameters);

            return SendMail(toEmail, fromEmail, subject, body, string.Empty);
        }

        public virtual bool SendMail([NotNull] string toEmail, [NotNull] string fromEmail, [NotNull] string subject, [NotNull] string body, [NotNull] string attachmentFileName)
        {
            Assert.ArgumentNotNull(toEmail, nameof(toEmail));
            Assert.ArgumentNotNull(fromEmail, nameof(fromEmail));
            Assert.ArgumentNotNull(subject, nameof(subject));
            Assert.ArgumentNotNull(body, nameof(body));
            Assert.ArgumentNotNull(attachmentFileName, nameof(attachmentFileName));

            _mailTo = toEmail;
            _mailFrom = fromEmail;
            _mailBody = body;
            _mailAttachmentFileName = attachmentFileName;
            _mailSubject = subject;

            return SendMail();
        }

        protected virtual bool SendMail()
        {
            var message = new MailMessage
            {
                From = new MailAddress(_mailFrom),
                Body = _mailBody,
                Subject = _mailSubject,
                IsBodyHtml = true
            };
            message.To.Add(_mailTo);

            if (_mailAttachmentFileName != null && File.Exists(_mailAttachmentFileName))
            {
                var attachment = new Attachment(_mailAttachmentFileName);
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