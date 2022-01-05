namespace shop.commerce.api.common
{
    using System.Collections.Generic;

    /// <summary>
    /// the piece join model
    /// </summary>
    public partial class AttachmentModel
    {
        /// <summary>
        /// the id of the file
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// the file as a byte[]
        /// </summary>
        public byte[] File { get; set; }

        /// <summary>
        /// the original name of the file including the extension
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// the mime-type of the file
        /// </summary>
        public string FileType { get; set; }
    }

    /// <summary>
    /// partial part for <see cref="AttachmentModel"/>
    /// </summary>
    public partial class AttachmentModel : System.IEquatable<AttachmentModel>
    {
        /// <summary>
        /// the string reApplication of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => $"name: {FileName}, type: {FileType}";

        /// <summary>
        /// check if the given object equals to this instant
        /// </summary>
        /// <param name="obj">the object to check</param>
        /// <returns>true if equals false if not</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(AttachmentModel)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return base.Equals(obj as AttachmentModel);
        }

        /// <summary>
        /// check if the given object equals to this instant
        /// </summary>
        /// <param name="other">the object to check</param>
        /// <returns>true if equals false if not</returns>
        public bool Equals(AttachmentModel other)
            => !(other is null) && 
            other.FileId == FileId && 
            other.FileName == FileName && 
            other.FileType == FileType;

        /// <summary>
        /// get the hash code of the entity
        /// </summary>
        /// <returns>the hash code value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 144377059;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileId);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileType);
                return hashCode;
            }
        }

        public static bool operator ==(AttachmentModel left, AttachmentModel right) => EqualityComparer<AttachmentModel>.Default.Equals(left, right);
        public static bool operator !=(AttachmentModel left, AttachmentModel right) => !(left == right);
    }
}
