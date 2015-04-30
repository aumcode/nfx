//this script should validate AdmissionDate field for NFX client technology
if (record.fldReligious)
    if (record.fldAdmissionDate.Year > 2008) throw "Religious people can not be admitted after 2008";