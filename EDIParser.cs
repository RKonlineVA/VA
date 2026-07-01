private void ParseNameSegment(dynamic segments, EDI275Document document)
{
    try
    {
        if (segments == null || document == null)
            return;

        document.PatientReports ??= new List<PatientReport>();

        // Handle both a single segment object or an enumerable of segments
        if (segments is System.Collections.IEnumerable enumerable && !(segments is string))
        {
            foreach (var segment in enumerable)
            {
                var patient = new PatientReport();

                // preserve original mapping but guard nulls
                patient.EntityIdentifierCode = segment?.NM1?.EntityIdentifierCode_01;
                patient.EntityTypeQualifier = segment?.NM1?.EntityTypeQualifier_02;
                patient.LastName = segment?.NM1?.NameLastorOrganizationName_03;
                patient.FirstName = segment?.NM1?.NameFirst_04;
                patient.MiddleName = segment?.NM1?.NameMiddle_05;
                patient.NamePrefix = segment?.NM1?.NamePrefix_06;
                patient.NameSuffix = segment?.NM1?.NameSuffix_07;
                patient.IdentificationCodeQualifier = segment?.NM1?.IdentificationCodeQualifier_08;
                patient.IdentificationCode = segment?.NM1?.IdentificationCode_09;

                patient.PatientName = string.Join(" ", new[]
                {
                    patient.FirstName,
                    patient.MiddleName,
                    patient.LastName
                }.Where(x => !string.IsNullOrWhiteSpace(x)));

                if (string.Equals(patient.IdentificationCodeQualifier, "MI", StringComparison.OrdinalIgnoreCase))
                    patient.PatientId = patient.IdentificationCode;

                // Set document.PatientReport to the first parsed patient if not set
                document.PatientReport ??= patient;

                document.PatientReports.Add(patient);

                _logger.LogInformation(
                    "Parsed NM1 - Entity: {Entity}, Last: {Last}, First: {First}, Qualifier: {Qualifier}, Id: {Id}",
                    patient.EntityIdentifierCode,
                    patient.LastName,
                    patient.FirstName,
                    patient.IdentificationCodeQualifier,
                    patient.IdentificationCode
                );
            }
        }
        else
        {
            // single segment case
            var segment = segments;
            var patient = new PatientReport();

            patient.EntityIdentifierCode = segment?.NM1?.EntityIdentifierCode_01;
            patient.EntityTypeQualifier = segment?.NM1?.EntityTypeQualifier_02;
            patient.LastName = segment?.NM1?.NameLastorOrganizationName_03;
            patient.FirstName = segment?.NM1?.NameFirst_04;
            patient.MiddleName = segment?.NM1?.NameMiddle_05;
            patient.NamePrefix = segment?.NM1?.NamePrefix_06;
            patient.NameSuffix = segment?.NM1?.NameSuffix_07;
            patient.IdentificationCodeQualifier = segment?.NM1?.IdentificationCodeQualifier_08;
            patient.IdentificationCode = segment?.NM1?.IdentificationCode_09;

            patient.PatientName = string.Join(" ", new[]
            {
                patient.FirstName,
                patient.MiddleName,
                patient.LastName
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.Equals(patient.IdentificationCodeQualifier, "MI", StringComparison.OrdinalIgnoreCase))
                patient.PatientId = patient.IdentificationCode;

            document.PatientReport ??= patient;
            document.PatientReports.Add(patient);

            //   
            _logger.LogInformation(
                "Parsed NM1 - Entity: {Entity}, Last: {Last}, First: {First}, Qualifier: {Qualifier}, Id: {Id}",
                patient.EntityIdentifierCode,
                patient.LastName,
                patient.FirstName,
                patient.IdentificationCodeQualifier,
                patient.IdentificationCode
            );
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error parsing NM1 segment");
    }
}private void ParseNameSegment(dynamic segments, EDI275Document document)
{
    try
    {
        if (segments == null || document == null)
            return;

        document.PatientReports ??= new List<PatientReport>();

        // Handle both a single segment object or an enumerable of segments
        if (segments is System.Collections.IEnumerable enumerable && !(segments is string))
        {
            foreach (var segment in enumerable)
            {
                var patient = new PatientReport();

                // preserve original mapping but guard nulls
                patient.EntityIdentifierCode = segment?.NM1?.EntityIdentifierCode_01;
                patient.EntityTypeQualifier = segment?.NM1?.EntityTypeQualifier_02;
                patient.LastName = segment?.NM1?.NameLastorOrganizationName_03;
                patient.FirstName = segment?.NM1?.NameFirst_04;
                patient.MiddleName = segment?.NM1?.NameMiddle_05;
                patient.NamePrefix = segment?.NM1?.NamePrefix_06;
                patient.NameSuffix = segment?.NM1?.NameSuffix_07;
                patient.IdentificationCodeQualifier = segment?.NM1?.IdentificationCodeQualifier_08;
                patient.IdentificationCode = segment?.NM1?.IdentificationCode_09;

                patient.PatientName = string.Join(" ", new[]
                {
                    patient.FirstName,
                    patient.MiddleName,
                    patient.LastName
                }.Where(x => !string.IsNullOrWhiteSpace(x)));

                if (string.Equals(patient.IdentificationCodeQualifier, "MI", StringComparison.OrdinalIgnoreCase))
                    patient.PatientId = patient.IdentificationCode;

                // Set document.PatientReport to the first parsed patient if not set
                document.PatientReport ??= patient;

                document.PatientReports.Add(patient);

                _logger.LogInformation(
                    "Parsed NM1 - Entity: {Entity}, Last: {Last}, First: {First}, Qualifier: {Qualifier}, Id: {Id}",
                    patient.EntityIdentifierCode,
                    patient.LastName,
                    patient.FirstName,
                    patient.IdentificationCodeQualifier,
                    patient.IdentificationCode
                );
            }
        }
        else
        {
            // single segment case
            var segment = segments;
            var patient = new PatientReport();

            patient.EntityIdentifierCode = segment?.NM1?.EntityIdentifierCode_01;
            patient.EntityTypeQualifier = segment?.NM1?.EntityTypeQualifier_02;
            patient.LastName = segment?.NM1?.NameLastorOrganizationName_03;
            patient.FirstName = segment?.NM1?.NameFirst_04;
            patient.MiddleName = segment?.NM1?.NameMiddle_05;
            patient.NamePrefix = segment?.NM1?.NamePrefix_06;
            patient.NameSuffix = segment?.NM1?.NameSuffix_07;
            patient.IdentificationCodeQualifier = segment?.NM1?.IdentificationCodeQualifier_08;
            patient.IdentificationCode = segment?.NM1?.IdentificationCode_09;

            patient.PatientName = string.Join(" ", new[]
            {
                patient.FirstName,
                patient.MiddleName,
                patient.LastName
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.Equals(patient.IdentificationCodeQualifier, "MI", StringComparison.OrdinalIgnoreCase))
                patient.PatientId = patient.IdentificationCode;

            document.PatientReport ??= patient;
            document.PatientReports.Add(patient);

            //   
            _logger.LogInformation(
                "Parsed NM1 - Entity: {Entity}, Last: {Last}, First: {First}, Qualifier: {Qualifier}, Id: {Id}",
                patient.EntityIdentifierCode,
                patient.LastName,
                patient.FirstName,
                patient.IdentificationCodeQualifier,
                patient.IdentificationCode
            );
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error parsing NM1 segment");
    }
}