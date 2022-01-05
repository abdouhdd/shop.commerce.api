namespace shop.commerce.api.common
{
    using System.Collections.Generic;

    /// <summary>
    /// the email template content
    /// </summary>
    public class EmailTemplate
    {
        /// <summary>
        /// the id of the email template
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// the content of the temple as Language => content
        /// </summary>
        /// <remarks>
        /// <para>content example</para>
        /// <code>
        /// {
        ///     es : {
        ///         Subject: "subject example",
        ///         Body: "body example"
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public Dictionary<string, EmailTemplateContent> Content { get; set; }

        /// <inheritdoc/>>
        public override string ToString()
            => $"{Id} => {Content.Count} Content";
    }

    /// <summary>
    /// the email content of the <see cref="EmailTemplate"/>
    /// </summary>
    public class EmailTemplateContent
    {
        /// <summary>
        /// the email subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// the body of the email
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// a flag to check if the body is an HTML body
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// set the place holders values to the <see cref="BodyContent"/> content
        /// </summary>
        /// <param name="placeHolders">the list of place holders</param>
        public void SetPlaceHolders(PlaceHolder[] placeHolders)
        {
            Body = PlaceHolder.Set(Body, placeHolders);
            Subject = PlaceHolder.Set(Subject, placeHolders);
        }
    }
}
