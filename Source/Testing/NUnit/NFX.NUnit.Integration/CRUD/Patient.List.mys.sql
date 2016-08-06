#pragma
modify=tbl_patient
key=counter
load=doctor_phone,doctor_id
ignore=marker
@last_name=lname
@first_name=fname
.last_name=This is description of last name

select 
 'X' as Marker,
 t1.`counter`,
 t1.`ssn`,
 t1.`lname` as last_name,
 t1.`fname` as first_name,
 t1.`DOB`,
 t1.`Address1`,
 t1.`Address2`,
 t1.`City`,
 t1.`State`,
 t1.`Zip`,
 t1.`Phone`,
 t1.`Amount`,
 t1.`Note`,
  
 t1.`c_doctor`, 
 t2.`phone` as doctor_phone, 
 t2.`NPI`	as doctor_id
from
 tbl_patient t1
  left outer join tbl_doctor t2 on t1.c_doctor = t2.counter
where
 t1.lname like ?LN