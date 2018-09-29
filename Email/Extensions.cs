using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace Utilities.Email
{
    public static class Extensions
    {

        /// <summary>
        /// Allows saving of file attachments from a MailMessage.
        /// </summary>
        /// <param name="message">The email.</param>
        /// <param name="filename">Tht file to save.</param>
        /// <see href="https://www.codeproject.com/KB/IP/smtpclientext.aspx?display=PrintAll&fid=1533533&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2876398"/>
        public static void Save(this MailMessage message, string filename)
        {
            Assembly assembly = typeof(SmtpClient).Assembly;

            Type mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

            using (FileStream _fileStream = new FileStream(filename, FileMode.Create)) {
                // Get reflection info for MailWriter contructor
                ConstructorInfo mailWriterContructor =
                    mailWriterType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(Stream) },
                        null
                );

                // Construct MailWriter object with our FileStream
                object mailWriter = mailWriterContructor.Invoke(new object[] { _fileStream });

                // Get reflection info for Send() method on MailMessage
                MethodInfo sendMethod = typeof(MailMessage).GetMethod(
                    "Send",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                // Call method passing in MailWriter
                sendMethod.Invoke(
                    message,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { mailWriter, true },
                    null
                );

                // Finally get reflection info for Close() method on our MailWriter
                MethodInfo closeMethod = mailWriter.GetType().GetMethod(
                    "Close",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                closeMethod.Invoke(
                    mailWriter,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { },
                    null
                );
            }
        }
    }
}
