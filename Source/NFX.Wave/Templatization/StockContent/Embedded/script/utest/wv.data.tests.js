    var run =  WAVE.UTest.run;
        var log =  WAVE.UTest.log;
        var logi =  WAVE.UTest.logi;
        var assertTrue = WAVE.UTest.assertTrue;
        var assertFalse = WAVE.UTest.assertFalse;

                function makeSimpleRecID(){
                  return new WAVE.RecordModel.Record("ID-123456", function(){
                                             new this.Field({Name: "FirstName", Type: "string"});
                                             new this.Field({Name: "LastName", Type: "string"});
                                             new this.Field({Name: "Age", Type: "int"}); });
                }

                function makeSimpleRecIDCreateFieldsLater(){
                  var rec = new WAVE.RecordModel.Record("ID-123456");
                      new rec.Field({Name: "FirstName", Type: "string"});
                      new rec.Field({Name: "LastName", Type: "string"});
                      new rec.Field({Name: "Age", Type: "int"}); 
                  return rec;
                }

                function makeSimpleRecInit(){
                 return new WAVE.RecordModel.Record(
                                            {ID: 'REC-1', 
                                             fields: [
                                              {def: {Name: 'FirstName', Type: 'string'}, val: 'Sunil'},
                                              {def: {Name: 'LastName', Type: 'string'}, val: 'Buchan'},
                                              {def: {Name: 'Age', Type: 'int'}, val: 127},
                                              {def: {Name: 'Helper', Type: 'string', Stored: false}}
                                             ]}
                                            );
                }


       run("Record", "allocbyId-load-recfld", function(){
         var rec = makeSimpleRecID();
         assertTrue( rec.loaded());

         assertTrue( rec.fldFirstName.loaded());
         assertTrue( rec.fldLastName.loaded());
         assertTrue( rec.fldAge.loaded());

         log( rec.toString() );
         assertTrue( "Record[ID-123456]" == rec.toString());

         rec.fldLastName.value("Dima");
         log( rec.fldLastName.toString());
         assertTrue( "[string]Field(LastName = 'Dima')" == rec.fldLastName.toString());
       });

       run("Record", "allocbyId-fldByName", function(){
         var rec = makeSimpleRecID();

         rec.fieldByName("FirstName").value("Egor");
         assertTrue( rec.fldFirstName.value() === rec.fieldByName("firstName").value());
         assertTrue( null===rec.fieldByName("does not exist"));
     });


       run("Record", "allocbyId-createfieldslater-load", function(){
         var rec = makeSimpleRecIDCreateFieldsLater();
         assertTrue( rec.loaded());

         assertTrue( rec.fldFirstName.loaded());
         assertTrue( rec.fldLastName.loaded());
         assertTrue( rec.fldAge.loaded());

         log( rec.toString() );
         assertTrue( "Record[ID-123456]" == rec.toString());

         rec.fldLastName.value("Dima");
         log( rec.fldLastName.toString());
         assertTrue( "[string]Field(LastName = 'Dima')" == rec.fldLastName.toString());
       });


       run("Record", "allocbyInit-load", function(){
         var rec = makeSimpleRecInit();
         assertTrue( rec.loaded());

         assertTrue( rec.fldFirstName.loaded());
         assertTrue( rec.fldLastName.loaded());
         assertTrue( rec.fldAge.loaded());

         log( rec.toString() );
         assertTrue( "Record[REC-1]" == rec.toString());

         rec.fldLastName.value("Dima");
         log( rec.fldLastName.toString());
         assertTrue( "[string]Field(LastName = 'Dima')" == rec.fldLastName.toString());
       });


       run("Record", "index-value-data-modified", function(){
         
         var rec = makeSimpleRecID();
         var fields = rec.fields();

         fields[0].value("Alex");
         fields[1].value("Borisov", true); //GUI
         fields[2].value(34);

         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Alex","LastName":"Borisov","Age":34}' == JSON.stringify(rec.data()));

         assertTrue(fields[0].isModified());
         assertFalse(fields[0].isGUIModified());

         assertTrue(fields[1].isModified());
         assertTrue(fields[1].isGUIModified()); //GUI

         assertTrue(fields[2].isModified());
         assertFalse(fields[2].isGUIModified());
       });

       run("Record", "name-value-data-modified", function(){
         
         var rec = makeSimpleRecID();

         rec.fldFirstName.value("Alex");
         rec.fldLastName.value("Borisov", true); //GUI
         rec.fldAge.value(34);

         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Alex","LastName":"Borisov","Age":34}' == JSON.stringify(rec.data()));

         assertTrue(rec.fldFirstName.isModified());
         assertFalse(rec.fldFirstName.isGUIModified());

         assertTrue(rec.fldLastName.isModified());
         assertTrue(rec.fldLastName.isGUIModified()); //GUI

         assertTrue(rec.fldAge.isModified());
         assertFalse(rec.fldAge.isGUIModified());
       });


       run("Record", "make-rec-init", function(){
         
         var rec = makeSimpleRecInit();

         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Sunil","LastName":"Buchan","Age":127}' == JSON.stringify(rec.data()));

         assertTrue( "REC-1"==rec.ID());

         assertFalse(rec.fldFirstName.isModified());
         assertFalse(rec.fldFirstName.isGUIModified());

         assertFalse(rec.fldLastName.isModified());
         assertFalse(rec.fldLastName.isGUIModified()); 

         assertFalse(rec.fldAge.isModified());
         assertFalse(rec.fldAge.isGUIModified());
       });

        run("Record", "data-withflags", function(){
         
         var rec = makeSimpleRecInit();

         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Sunil","LastName":"Buchan","Age":127}' == JSON.stringify(rec.data()));

         //modified only
         log( JSON.stringify(rec.data(true)));
         assertTrue( '{}' == JSON.stringify(rec.data(true)));

         rec.fldLastName.value("Gagarin");
         
         //modified only
         log( JSON.stringify(rec.data(true)));
         assertTrue( '{"LastName":"Gagarin"}' == JSON.stringify(rec.data(true)));

         //any
         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Sunil","LastName":"Gagarin","Age":127}' == JSON.stringify(rec.data()));

         //any with non-stored fields
         log( JSON.stringify(rec.data(false, true)));
         assertTrue( '{"FirstName":"Sunil","LastName":"Gagarin","Age":127,"Helper":null}' == JSON.stringify(rec.data(false, true)));

       });


       run("Record", "add-drop-fields", function(){
         
         var rec = makeSimpleRecInit();

         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Sunil","LastName":"Buchan","Age":127}' == JSON.stringify(rec.data()));

         rec.fldLastName.drop();

         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Sunil","Age":127}' == JSON.stringify(rec.data()));
         assertTrue( typeof(rec.fldLastName)===WAVE.UNDEFINED);

         new rec.Field({Name: "Zombi", Type: "bool"});
         rec.fldZombi.value(false);
         
         log( JSON.stringify(rec.data()));
         assertTrue( '{"FirstName":"Sunil","Age":127,"Zombi":false}' == JSON.stringify(rec.data()));
       });

       run("Record", "add-drop-fields_withRecEvents", function(){
         
         var rec = makeSimpleRecInit();

         var elog = "";
         rec.eventBind(WAVE.RecordModel.EVT_FIELD_DROP, function(rec, field, phase){
          elog += rec.ID() + field.name() + phase;
         });
         rec.fldLastName.drop();
         log( elog);
         assertTrue( "REC-1LastNamebeforeREC-1LastNameafter"==elog);
       });

       run("Record", "add-drop-fields_withFieldEvents", function(){
         
         var rec = makeSimpleRecInit();

         var elog = "";
         
         rec.fldLastName.eventBind(WAVE.RecordModel.EVT_FIELD_DROP, function(field, phase){
          elog += field.name() + phase;
         });
         rec.fldLastName.drop();
         log( elog);
         assertTrue( "LastNamebeforeLastNameafter"==elog);
       });

        run("Record", "fields-withAnyFieldEvents-deferedValidation", function(){
         
         var rec = makeSimpleRecInit();

         var elog = "";
         rec.fldLastName.deferValidation(true);
         rec.fldLastName.eventBind(WAVE.EventManager.ANY_EVENT, function(evtType, field, phase){
          elog += "|"+evtType + field.name() + phase + field.value();
         });
         rec.fldLastName.value("Korolev");
         log( elog);
         assertTrue( "|data-changeLastNamebeforeBuchan|data-changeLastNameafterKorolev"==elog);
       });

       run("Record", "fields-withAnyFieldEvents-NOTdeferedValidation", function(){
         
         var rec = makeSimpleRecInit();

         var elog = "";
         rec.fldLastName.deferValidation(false);
         rec.fldLastName.eventBind(WAVE.EventManager.ANY_EVENT, function(evtType, field, phase){
          elog += "|"+evtType + field.name() + phase + field.value();
         });
         rec.fldLastName.value("Korolev");
         log( elog);
         assertTrue( "|data-changeLastNamebeforeBuchan|validateLastNameundefinedKorolev|validatedLastNameundefinedKorolev|data-changeLastNameafterKorolev"==elog);
       });

       run("Validation", "field-required-string", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string"});
                                             new this.Field({Name: "B", Type: "string", Required: true});
                                             new this.Field({Name: "C", Type: "int"}); });
          
          assertFalse( rec.fldB.validated());
          rec.fldB.validate();
          assertTrue( rec.fldB.validated());
          assertFalse( rec.fldB.valid());
          log( rec.fldB.validationError());


          rec.fldB.value("aaaaa");
          assertTrue( rec.fldB.valid());

       });

       run("Validation", "field-required-int", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "C", Type: "int", Required: true}); });
          
          assertFalse( rec.fldC.validated());
          rec.fldC.validate();
          assertTrue( rec.fldC.validated());
          assertFalse( rec.fldC.valid());
          log( rec.fldC.validationError());

          rec.fldC.value(-10);
          assertTrue( rec.fldC.valid());

       });

       run("Validation", "rec-validate-allerrors", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string"});
                                             new this.Field({Name: "B", Type: "string", Required: true});
                                             new this.Field({Name: "C", Type: "int", Required: true}); });
          
          rec.validate();
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "'B' must have a value") );
          assertTrue( WAVE.strContains(all, "'C' must have a value") );
       });

     
       run("Validation", "field-required-int-min-max", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "Age", Type: "int", Required: true, MinValue: 10, MaxValue: 150}); });
          
          assertFalse( rec.validated());
          assertFalse( rec.valid());
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
         
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "'Age' must have a value"));

          rec.fldAge.value(120);
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());

          rec.fldAge.value(180);
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "be greater than '150'"));

          rec.fldAge.value(2);
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "be less than '10'"));

          rec.fldAge.value(98);
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());
       });

       run("Validation", "field-required-real-min-max", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "ZDZ", Type: "real", Required: true, MinValue: -123.1, MaxValue: 217.455}); });
          
          assertFalse( rec.validated());
          assertFalse( rec.valid());
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
         
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "'ZDZ' must have a value"));

          rec.fldZDZ.value(120.83);
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());

          rec.fldZDZ.value(217.55);
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "be greater than '217.455'"));

          rec.fldZDZ.value(-123.9);
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "be less than '-123.1'"));

          rec.fldZDZ.value(98);
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());
       });



       run("Validation", "field-required-size", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "Name", Type: "string", Required: true, Size: 10}); });
          
          assertFalse( rec.validated());
          assertFalse( rec.valid());
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
         
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "'Name' must have a value"));

          rec.fldName.value("0123456789");
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());

          rec.fldName.value("01234567890");
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "be longer than 10"));

          rec.fldName.value("Short");
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());
       });


       run("Conversion", "set-get-various", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "Name", Type: "string", Required: true, Size: 10});
                                             new this.Field({Name: "Registered", Type: "bool", Required: true});
                                             new this.Field({Name: "Age", Type: "int"});
                                             new this.Field({Name: "Balance", Type: "money"});
                                             new this.Field({Name: "DOB", Type: "datetime"});
                                             });
          rec.fldName.value("Someone");
          assertTrue("Someone" === rec.fldName.value());

          rec.fldRegistered.value("true");
          assertTrue( true === rec.fldRegistered.value());
          rec.fldRegistered.value(0);
          assertTrue( false === rec.fldRegistered.value());
          rec.fldRegistered.value(true);
          assertTrue( true === rec.fldRegistered.value());

          rec.fldAge.value("12");
          assertTrue( 12 === rec.fldAge.value());
          rec.fldAge.value(128);
          assertTrue( 128 === rec.fldAge.value());

          rec.fldBalance.value("120000000.49");
          assertTrue( 120000000.49 === rec.fldBalance.value());
          rec.fldBalance.value("678900.99");
          assertTrue( 678900.99 === rec.fldBalance.value());

          rec.fldDOB.value("2/1/20012 14:49");
          assertTrue( new Date("2/1/20012 14:49").getTime() === rec.fldDOB.value().getTime());
          rec.fldDOB.value(new Date("2/1/20012 14:49"));
          assertTrue( new Date("2/1/20012 14:49").getTime() === rec.fldDOB.value().getTime());
          
       });

       run("Validation", "field-required-lookupdict", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "CareType", Type: "string", Required: true, LookupDict: {HHC: "Home health care", PVT: "Private Care"}}); });
          
          assertFalse( rec.validated());
          assertFalse( rec.valid());
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
         
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "'CareType' must have a value"));

          rec.fldCareType.value("HHC");
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());

          rec.fldCareType.value("NTZ");
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "value 'NTZ' is not allowed"));

          rec.fldCareType.value("PVT");
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());
       });

       run("Validation", "field-nonrequired-lookupdict", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "CareType", Type: "string", Required: false, LookupDict: {HHC: "Home health care", PVT: "Private Care"}}); });
          
          assertFalse( rec.validated());
          assertFalse( rec.valid());
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());
         
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( all === "");

          rec.fldCareType.value("HHC");
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());

          rec.fldCareType.value("NTZ");
          rec.validate();
          assertTrue( rec.validated());
          assertFalse( rec.valid());
          var all = rec.allValidationErrorStrings();
          log( all);
          assertTrue( WAVE.strContains(all, "value 'NTZ' is not allowed"));
    
          rec.fldCareType.value("PVT");
          rec.validate();
          assertTrue( rec.validated());
          assertTrue( rec.valid());
       });


       run("TextCase", "asis", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string", Case: WAVE.RecordModel.CASE_ASIS});
                                             });
          
          rec.fldA.value("aBc d", true);
          assertTrue( "aBc d" === rec.fldA.value());
       });

       run("TextCase", "upper", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string", Case: WAVE.RecordModel.CASE_UPPER});
                                             });
          
          rec.fldA.value("aBc deF", true);
          assertTrue( "ABC DEF" === rec.fldA.value());
       });

       run("TextCase", "lower", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string", Case: WAVE.RecordModel.CASE_LOWER});
                                             });
          
          rec.fldA.value("aBc deF", true);
          assertTrue( "abc def" === rec.fldA.value());
       });

       run("TextCase", "capitalize", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string", Case: WAVE.RecordModel.CASE_CAPS});
                                             });
          
          rec.fldA.value("aBc DeF foR mE", true);
          log (rec.fldA.value());
          assertTrue( "ABc DeF FoR ME" === rec.fldA.value());
       });

       run("TextCase", "capitalize+normalize", function(){
          var rec = new WAVE.RecordModel.Record("1", function(){
                                             new this.Field({Name: "A", Type: "string", Case: WAVE.RecordModel.CASE_CAPSNORM});
                                             });
          
          rec.fldA.value("aBc DeF foR mE", true);
          log (rec.fldA.value());
          assertTrue( "Abc Def For Me" === rec.fldA.value());
       });

   
        var REC = null;
        var RVIEW = null;
        run("RecordView", "build", function(){
         REC =  new WAVE.RecordModel.Record(
                                            {ID: 'R1', 
                                             fields: [
                                              {def: {Name: 'FirstName', Type: 'string', Required: true}, val: 'Sunil'},
                                              {def: {Name: 'LastName', Type: 'string', Required: true}, val: 'Buchan'},
                                              {def: {Name: 'Age', Type: 'int', MinValue: 10}, val: 127},
                                              {def: {Name: 'Registered', Type: 'bool'}, val: true},
                                              {def: {Name: 'MusicType', Type: 'string', LookupDict: {HRK: "Hard Rock", CRK: "Classic Rock", RAP: "Rap", CMU: "Classical music"}}},
                                              {def: {Name: 'Various', Type: 'string',
                                                     LookupDict: {CAR: "Has a car",
                                                                  SMK: "Smokes",
                                                                  PAR: "Party member",
                                                                  LAL: "Lived alone",
                                                                  HMD: "Hates mobile devices",
                                                                  FAS: "Former Alaskan Subsidary",
                                                                  SEX: "Space explorer"}}}
                                             ]}
                                            );
         RVIEW = new WAVE.RecordModel.RecordView("V1", REC);
      });

