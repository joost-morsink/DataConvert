namespace Biz.Morsink.DataConvert.Test
{

    public class Email
    {
        public Email(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static implicit operator string(Email email)
            => email.Value;

        public static implicit operator Email(string email)
            => new Email(email);

        public static bool TryConvert(string value, out Email email)
        {
            if (value.Contains("@"))
            {
                email = new Email(value);
                return true;
            }

            email = null;
            return false;
        }
    }
}