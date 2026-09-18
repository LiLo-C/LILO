namespace Lilo.Config
{
    /// <summary>One validation failure: which field, what value, how to fix it. Never a silent default.</summary>
    public readonly struct ConfigValidationFailure
    {
        public readonly string FieldName;
        public readonly string InvalidValue;
        public readonly string CorrectionRule;

        public ConfigValidationFailure(string fieldName, string invalidValue, string correctionRule)
        {
            FieldName = fieldName;
            InvalidValue = invalidValue;
            CorrectionRule = correctionRule;
        }

        public override string ToString() => $"{FieldName} = {InvalidValue} ({CorrectionRule})";
    }
}
