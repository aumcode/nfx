
        var run =  WAVE.UTest.run;
        var log =  WAVE.UTest.log;
        var logi =  WAVE.UTest.logi;
        var assertTrue = WAVE.UTest.assertTrue;
        var assertFalse = WAVE.UTest.assertFalse;
        var marginOfError = 0.000001;

        // deviation Ok in other words modul of argument difference less than marginOfError
        function devOk(a, b) { return Math.abs(a-b) < marginOfError; }

       run("Assertions", "assertTrue", function(){
         assertTrue(true);
         log("This must pass 1");
         log("This must pass 2");
         log("This must pass 3");
         log("This must pass 4");
         log("This must pass 5");
       });

       run("Assertions", "testingStarted", function(){
         assertTrue( WAVE.UTest.testingStarted() );
       });

       run("Assertions", "assertTrue", function(){
         assertTrue(false, "This was expected to fail and MUST BE SHOWN in RED as we test the unit tester itself!");
         log("This will never show");
       });


       run("Dummy", "falsness", function(){
         assertFalse( WAVE.falseness() );
       });

       run("Dummy", "falsnefunc", function(){
         assertFalse( WAVE.falsefunc() );
       });



       run("Localizer", "allLanguageISOs", function(){
         var array = ['eng', 'rus', 'deu', 'fra', 'esp'];
         
         var keys = WAVE.LOCALIZER.allLanguageISOs(); 

         assertTrue( WAVE.isSame(array, keys));
       });

      
       WAVE.LOCALIZER.rus = {"--ANY-SCHEMA--": { "--ANY-FIELD--": {"Java Script is crap!": "Ява Скрипт - Говно!"}}};
      
       run("Localizer", "strLocalize", function(){

         var val = WAVE.strLocalize('rus', 'tezt', 'fld', 'Java Script is crap!')

         assertTrue( "Ява Скрипт - Говно!" === val);
       });



       run("Arrays", "arrayDelete", function(){
         var array = ['Alex', 'Boris', 'Doris'];
         assertTrue( true === WAVE.arrayDelete(array, 'Boris'));
         assertTrue( 2 === array.length);
         assertTrue( 'Alex' === array[0]);
         assertTrue( 'Doris' === array[1]);
       });

       run("Arrays", "arrayClear", function(){
         var array = [1,2,3];
         assertTrue( 3 === array.length);
         
         WAVE.arrayClear(array);

         assertTrue( 0 === array.length);
       });

       run("Arrays", "mergeArrays", function () {
         assertTrue(WAVE.mergeArrays().length === 0);
         assertTrue(WAVE.isSame(WAVE.mergeArrays(), []));

         var arrayA = [1, 2, 3];
         var arrayB = [2, 3, 5, 7];
         var r = WAVE.mergeArrays(arrayA, arrayB); 

         assertTrue(r.length === 5);
         assertTrue(WAVE.isSame(r, [1, 2, 3, 5, 7]));

         arrayB = [2, 3, 5, 7];
         var r = WAVE.mergeArrays(arrayB); 

         assertTrue(r.length === 4);
         assertTrue(WAVE.isSame(r, [2, 3, 5, 7]));


         arrayA = ["rus", "RUS", "ENg"];
         arrayB = ["Eng", "eps"];

         r = WAVE.mergeArrays(arrayA, arrayB);
         assertTrue(r.length === 5);
         assertTrue(WAVE.isSame(r, ["rus", "RUS", "ENg", "Eng", "eps"]));

         r = WAVE.mergeArrays(arrayA, arrayB, function (a, b) { return a.toUpperCase() === b.toUpperCase() });
         assertTrue(r.length === 3);
         assertTrue(WAVE.isSame(r, ["rus", "ENg", "eps"]));

         r = WAVE.mergeArrays(arrayA, arrayB, function(a, b) {return a === b;}, function(a){return a.toLowerCase();});
         assertTrue(r.length === 5);
         assertTrue(WAVE.isSame(r, ["rus", "rus", "eng", "eng", "eps"]));

         var i = 0;
         r = WAVE.mergeArrays(arrayA, arrayB, function (a, b) { return a.toUpperCase() === b.toUpperCase() }, function() {i++; return i;} );
         assertTrue(r.length === 3);
         assertTrue(WAVE.isSame(r, [1, 2, 3]));
       });

       run("Arrays", "arrayShallowCopy", function(){
         var array = [1,2,3];
         var copy = WAVE.arrayShallowCopy(array);
         
         assertTrue( 3 === copy.length);
         assertTrue( 1 === copy[0]);
         assertTrue( 2 === copy[1]);
         assertTrue( 3 === copy[2]);
       });

       run("Arrays", "inArray", function(){
         var array = ['Alex', 'Boris', 'Doris'];
         assertTrue( WAVE.inArray(array, 'Alex'));
         assertTrue( WAVE.inArray(array, 'Boris'));
         assertTrue( WAVE.inArray(array, 'Doris'));
         assertFalse( WAVE.inArray(array, 'Joseph'));
       });
      

       run("Objects", "tryParseJSON", function(){

         assertFalse( WAVE.tryParseJSON().ok );
         assertFalse( WAVE.tryParseJSON(null).ok );

         assertTrue( WAVE.tryParseJSON({}).ok );
         assertTrue( WAVE.tryParseJSON("{}").ok );

         assertTrue( WAVE.tryParseJSON({a: 4}).ok );
         assertTrue( WAVE.tryParseJSON('{"a": 4}').ok );

         assertTrue( 4 === WAVE.tryParseJSON({a: 4}).obj.a );
         assertTrue( 4 === WAVE.tryParseJSON('{"a": 4}').obj.a );


         assertTrue( WAVE.tryParseJSON([8, -3]).ok );
         assertTrue( WAVE.tryParseJSON("[8, -3]").ok );

         assertTrue( -3 === WAVE.tryParseJSON([8, -3]).obj[1] );
         assertTrue( -3 === WAVE.tryParseJSON("[8, -3]").obj[1] );

         assertFalse( WAVE.tryParseJSON(function(){}).ok );
         
         assertTrue(null === WAVE.tryParseJSON(null, null).obj, "a1");
         assertTrue(null !== WAVE.tryParseJSON(null).obj, "a2");
         assertTrue(78 === WAVE.tryParseJSON(null,{a: 78}).obj.a, "a3");
       });


       run("Objects", "checkKeysUnique", function(){
         assertTrue(null === WAVE.checkKeysUnique(null));
         assertTrue(undefined === WAVE.checkKeysUnique(undefined));

         assertFalse(WAVE.checkKeysUnique({a:2 ,b:1}));
         assertFalse(WAVE.checkKeysUnique({a:1, b:1, c:2, d:3}));
         assertFalse(WAVE.checkKeysUnique({a:4, b:1, c:2, d:3, e:2, f:{}, g:3}));

         assertTrue(WAVE.checkKeysUnique({a:3, A:1}));
         assertTrue(WAVE.checkKeysUnique({a:3, b:1, B: {}, A: "2"}));
       });


       run("Objects", "memberClone_object", function(){
         assertTrue(4 === WAVE.memberClone({a: 4}).a );
         assertTrue(4 === WAVE.memberClone({A: 4}).A );
         assertTrue(4 === WAVE.memberClone({A: 4}, true).a );

         var obj = {B:{A:54}};
         var res = WAVE.memberClone(obj);
         assertTrue(54 === res.B.A);
         obj = {B:{A:54}};
         res = WAVE.memberClone(obj, true);
         assertTrue(54 === res.b.a);

         obj = {A:{B:{c:21,E:100}, D:"pol"},f:true};
         res = WAVE.memberClone(obj);
         assertTrue(res.A.B.c === 21);
         assertTrue(res.A.B.E === 100);
         assertTrue(res.A.D === "pol");
         assertTrue(res.f);
         
         obj = {A:{B:{c:21,E:100}, D:"pol"},f:true};
         res = WAVE.memberClone(obj, true);
         assertTrue(res.a.b.c === 21);
         assertTrue(res.a.b.e === 100);
         assertTrue(res.a.d === "pol");
         assertTrue(res.f);
       });

       run("Objects", "memberClone_array", function(){
         assertTrue(2 === WAVE.memberClone([1,8]).length );
         assertTrue(3 === WAVE.memberClone([1,2,5]).length );
         assertTrue(5 === WAVE.memberClone([1,2,{a:7}, 8,9]).length );

         var obj = [1,2,{A: 77, b: -3}, true];
         var res = WAVE.memberClone(obj);
         assertTrue( 4 === res.length );
         assertTrue( WAVE.isArray(res) );
         assertTrue( WAVE.isObject(res[2]) );
         assertTrue( 2 === res[1] );
         assertTrue( -3 === res[2].b );
         assertTrue( res[3] );
         assertTrue( 77 === res[2].A );

         res = WAVE.memberClone(obj, true);
         assertTrue( 4 === res.length );
         assertTrue( WAVE.isArray(res) );
         assertTrue( WAVE.isObject(res[2]) );
         assertTrue( 2 === res[1] );
         assertTrue( -3 === res[2].b );
         assertTrue( res[3] );
         assertTrue( 77 === res[2].a );
       });

       run("Objects", "memberClone_mixed", function(){
         var obj = [1, {A : [{a:30}, false], "av": 3 }, "arr"];
         var res = WAVE.memberClone(obj);
         assertTrue( 30 === res[1].A[0].a );
         assertFalse( 30 === res[1].A[1] );
         assertTrue( 3 === res[1].av );
         assertTrue( "arr" === res[2] );

         res = WAVE.memberClone(obj, true);
         assertTrue( 30 === res[1].a[0].a );
         assertFalse( 30 === res[1].a[1] );
         assertTrue( 3 === res[1].av );
         assertTrue( "arr" === res[2] )
       });


       run("Objects", "EMPTY", function(){
         assertTrue( WAVE.empty({}), "kakashka-3" );
         assertTrue( WAVE.empty(null), "kakashka-2" );
         assertTrue( WAVE.empty(), "kakashka-1" );

         assertFalse( WAVE.empty({a:6}), "kakashka0");
         var ctor = function(){};
         ctor.prototype.a = 71;
         var obj = new ctor();
         assertFalse( WAVE.empty(obj), "kakashka");
       });


       run("Objects", "HAS", function(){
         var on = null;
         var o1 = {a: 1, b: 2};
         var o2 = [1,2,3];
         var o3 = function(){return true;};

         assertFalse( WAVE.has(null, 'a') );
         assertFalse( WAVE.has(on, 'a') );
         assertFalse( WAVE.has(o1, 'z') );
         assertTrue( WAVE.has(o1, 'a') );
         assertTrue( WAVE.has(o1, 'b') );

         assertFalse( WAVE.has(o2, 'a') );
         assertTrue( WAVE.has(o2, 1) );

         assertFalse( WAVE.has(o3, 'a') );
         assertFalse( WAVE.has(o3, 1) );
       });

       run("Objects", "GET", function(){
         var on = null;
         var o1 = {a: 1, b: 2};
         var o2 = [1,2,3];

         assertTrue( null === WAVE.get() );
         assertTrue( null === WAVE.get(on, null) );
         assertTrue( null === WAVE.get(on, "a") );
         assertTrue( 123 === WAVE.get(on, "a", 123) );

         assertTrue( null === WAVE.get(o1, null) );
         assertTrue( 1 === WAVE.get(o1, "a") );
         assertTrue( 1 === WAVE.get(o1, "a", 123) );
         assertTrue( 2 === WAVE.get(o1, "b") );
         assertTrue( 2 === WAVE.get(o1, "b", 123) );

         assertTrue( null === WAVE.get(o1, "c") );
         assertTrue( 123 === WAVE.get(o1, "c", 123) );

         assertTrue( 2 === WAVE.get(o2, 1) );
         assertTrue( 3 === WAVE.get(o2, 2, 123) );
         assertTrue( 123 === WAVE.get(o2, 3, 123) );

         var s = {};
         s.n = undefined;
         assertTrue( 100 === WAVE.get(s, "n", 100) );
       });


       run("Objects", "isObject", function(){
         var on = null;
         var o1 = {a: 1, b: 2};
         var o2 = [1,2,3];
         var o3 = function(){return true;};

         assertFalse( WAVE.isObject(on));
         assertTrue ( WAVE.isObject(o1) );
         assertFalse( WAVE.isObject(o2) );
         assertFalse( WAVE.isObject(o3) );
       });

       run("Objects", "isArray", function(){
         var on = null;
         var o1 = {a: 1, b: 2};
         var o2 = [1,2,3];
         var o3 = function(){return true;};

         assertFalse ( WAVE.isArray(on) );
         assertFalse ( WAVE.isArray(o1) );
         assertTrue  ( WAVE.isArray(o2) );
         assertFalse ( WAVE.isArray(o3) );
       });


       run("Objects", "isFunction", function(){
         var on = null;
         var o1 = {a: 1, b: 2};
         var o2 = [1,2,3];
         var o3 = function(){return true;};

         assertFalse( WAVE.isFunction(on) );
         assertFalse( WAVE.isFunction(o1) );
         assertFalse( WAVE.isFunction(o2) );
         assertTrue ( WAVE.isFunction(o3) );
       });

       run("Objects", "each-iteration-object", function() {
         var obj = { a: 1, b: true, c: 20.17, d: 'lenin'};
         var sum = '';
         WAVE.each(obj, function (v, k) {
           sum += (v + "[" + k + "], ");
         });
         assertTrue( sum === "1[a], true[b], 20.17[c], lenin[d], " );
       });

       run("Objects", "each-iteration-array", function() {
         var obj = [ 1, true, 20.17, 'lenin'];
         var sum = '';
         WAVE.each(obj, function (v, k) {
           sum += (v + "[" + k + "], ");
         });
         assertTrue( sum === "1[0], true[1], 20.17[2], lenin[3], " );
       });

       run("Objects", "extend-merge", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {c: 3, d: 4};

         var extended = WAVE.extend(o1, o2);

         assertTrue( 1 === extended.a);
         assertTrue( 2 === extended.b);
         assertTrue( 3 === extended.c);
         assertTrue( 4 === extended.d);
       });

       run("Objects", "extend-override", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {b: 129, c: 3, d: 4};

         var extended = WAVE.extend(o1, o2);

         assertTrue( 1 === extended.a);
         assertTrue( 129 === extended.b);
         assertTrue( 3 === extended.c);
         assertTrue( 4 === extended.d);
       });

       run("Objects", "extend-keep", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {b: 129, c: 3, d: 4};

         var extended = WAVE.extend(o1, o2, true);

         assertTrue( 1 === extended.a);
         assertTrue( 2 === extended.b);
         assertTrue( 3 === extended.c);
         assertTrue( 4 === extended.d);
       });

       run("Objects", "extend-keep-null", function(){
         var o1 = {a: 1, b: null};
         var o2 = {b: 129, c: 3, d: 4};

         var extended = WAVE.extend(o1, o2, true);

         assertTrue( 1 === extended.a);
         assertTrue( null === extended.b);
         assertTrue( 3 === extended.c);
         assertTrue( 4 === extended.d);
       });


       run("Objects", "isSame-1", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {a: 1, b: 2};
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-2", function(){
         var o1 = {a: 1, b: 12};
         var o2 = {a: 1, b: 2};
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-3", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {a: 1, b: 12};
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-4", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {a: 1, boba: 2};
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-5", function(){
         var o1 = {a: 1, boba: 2};
         var o2 = {a: 1, boba: 2};
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-6", function(){
         var o1 = {a: 1, boba: 2};
         var o2 = null;
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-7", function(){
         var o1 = [1,2,3,4,5,6,7];
         var o2 = [1,2,3,4,5,6,7];
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-8", function(){
         var o1 = [1,2,3,4,5,6,7];
         var o2 = [1,2,3,4,5,6,7,8];
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-9", function(){
         var o1 = [1,1,3,4,5,6,7,8];
         var o2 = [1,2,3,4,5,6,7,8];
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-10", function(){
         var o1 = [1,2,3,4,5,6,7,[]];
         var o2 = [1,2,3,4,5,6,7,[]];
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-11", function(){
         var o1 = [1,2,3,4,5,6,7,[]];
         var o2 = [1,2,3,4,5,6,7,[1]];
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-12", function(){
         var o1 = [1,2,3,4,5,6,7,[1,{a: 1}]];
         var o2 = [1,2,3,4,5,6,7,[1,{a: 1}]];
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-13", function(){
         var o1 = [1,2,3,4,5,6,7,[1,{a: 1, b:'Hello'}]];
         var o2 = [1,2,3,4,5,6,7,[1,{a: 1 }]];
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

        run("Objects", "isSame-14", function(){
         var o1 = [1,2,3,4,5,6,7,[1,{a: 1}]];
         var o2 = [1,2,3,4,5,6,7,[1,{a: 1, b:'Hello' }]];
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });


       run("Objects", "isSame-15", function(){
         var o1 = [1,2,3,4,5,6,7,[1,{a: 1, b:'Hello'}]];
         var o2 = [1,2,3,4,5,6,7,[1,{a: 1, b:'Hello'}]];
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-16", function(){
         var o1 = {a: 1, b: 2};
         var o2 = {a: 1, b: 2, c: 3};
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-17", function(){
         var o1 = {a: 1, b: 2, c: 3};
         var o2 = {a: 1, b: 2, c: 3};
        

         assertFalse( o1 === o2 );
         assertTrue( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-18", function(){
         var o1 = {a: 1, b: 2, c: 3};
         var o2 = {a: 1, b: 2, c: true};
        

         assertFalse( o1 === o2 );
         assertFalse( WAVE.isSame(o1, o2) );
       });

       run("Objects", "isSame-primitives", function(){
         assertTrue( WAVE.isSame(1 , 1) );
         assertFalse( WAVE.isSame(1 , 2) );
         
         assertTrue( WAVE.isSame(12.1 , 12.1) );
         assertFalse( WAVE.isSame(12.1 , 12.2) );
        
         assertTrue( WAVE.isSame(true , true) );
         assertTrue( WAVE.isSame(false , false) );
         assertFalse( WAVE.isSame(true , false) );
         
         assertTrue( WAVE.isSame("da" , "da") );
         assertFalse( WAVE.isSame("da" , "net") );

         assertTrue( WAVE.isSame( new Date("1/1/2011 14:00") ,new Date("1/1/2011 14:00")) );
         assertFalse( WAVE.isSame( new Date("1/1/2011 14:00") ,new Date("2/2/2011 14:01")) );

       });


       run("Objects", "isSame-withNull", function(){
         assertTrue( WAVE.isSame(null , null) );
         assertFalse( WAVE.isSame(1 , null) );
         assertFalse( WAVE.isSame(null ,1) );
       });






       run("Objects", "clone-1", function(){
         var o1 = {a: 1, b: 2};
         var o2 = WAVE.clone(o1);
                                   
         assertFalse( o1 === o2);
         assertTrue( 1 === o2.a);
         assertTrue( 2 === o2.b);
       });



       run("Objects", "clone-2", function(){
         var o1 = {a: 1, b: 2, c: {FirstName: 'Alex', LastName: 'Borisov'}};
         var o2 = WAVE.clone(o1);
                                   
         assertFalse( o1 === o2);
         assertTrue( 1 === o2.a);
         assertTrue( 2 === o2.b);
         assertTrue( 'Alex' === o2.c.FirstName);
         assertTrue( 'Borisov' === o2.c.LastName);
       });

       run("Objects", "clone-3", function(){
         var o1 = {a: 1, b: 2, c: {FirstName: 'Alex', LastName: 'Borisov', Contacts: ['sister','cousin']}};
         var o2 = WAVE.clone(o1);
                                   
         assertFalse( o1 === o2);
         assertTrue( 1 === o2.a);
         assertTrue( 2 === o2.b);
         assertTrue( 'Alex' === o2.c.FirstName);
         assertTrue( 'Borisov' === o2.c.LastName);
         assertTrue( 2 === o2.c.Contacts.length);
         assertTrue( 'sister' === o2.c.Contacts[0]);
         assertTrue( 'cousin' === o2.c.Contacts[1]);
       });

       run("Objects", "isScalar", function(){
         assertTrue( WAVE.isScalar(1));
         assertTrue( WAVE.isScalar(1.12));
         assertTrue( WAVE.isScalar(true));
         assertTrue( WAVE.isScalar("Hello!"));
         assertFalse( WAVE.isScalar([1,2,3]));
         assertFalse( WAVE.isScalar({a: 1, b: 2}));
       });

           function TestShape(name){
            this.Name = name;
            this.about = function(){ return "TestShape "+this.Name;};
           }


       run("Objects", "function-override", function(){
          var shape1 = new TestShape("Amorph1");
          var shape2 = new TestShape("Amorph2");

          assertTrue( "TestShape Amorph1" === shape1.about() );
          assertTrue( "TestShape Amorph2" === shape2.about() );

          shape2.about = WAVE.overrideFunction(shape2.about, function(){ return this.baseFunction() + "overridden"; });
          
          assertTrue( "TestShape Amorph1" === shape1.about() );         
          log( shape2.about());
          assertTrue( "TestShape Amorph2overridden" === shape2.about() );         
       });


       run("Objects", "propStrAsObject", function(){
         var o1 = {a: 1, b: '{"c": "yes", "d": "no"}'};
        
         assertFalse( WAVE.isObject(o1.b));
         WAVE.propStrAsObject(o1, "b");
         assertTrue( WAVE.isObject(o1.b));

         assertTrue( "yes"===o1.b.c);
         assertTrue( "no"===o1.b.d);
                    
       });

       run("Objects", "propStrAsObject-null", function(){
         var o1 = {a: 1, b: null};
        
         assertFalse( WAVE.isObject(o1.b));
         WAVE.propStrAsObject(o1, "b");
         assertFalse( WAVE.isObject(o1.b));
       });

       run("Objects", "propStrAsObject-undefined", function(){
         var o1 = {a: 1, z: 123};
        
         assertFalse( WAVE.isObject(o1.b));
         WAVE.propStrAsObject(o1, "b");
         assertFalse( WAVE.isObject(o1.b));
       });



       run("Integers", "tryParseInt", function(){
         var r = WAVE.tryParseInt();      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt(null);      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt({1:1});
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt("");
         assertTrue( r.ok === false && isNaN(r.value) );

         r = WAVE.tryParseInt(1);
         assertTrue( r.ok === true && r.value === 1 );
         r = WAVE.tryParseInt("1");      
         assertTrue( r.ok === true && r.value === 1 ); 
         r = WAVE.tryParseInt("1   3");      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt("1.2");      
         assertTrue( r.ok === false && r.value === 1.2 );
         r = WAVE.tryParseInt("1,2");      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt("eee34");      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt("34eee");      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt("some text");      
         assertTrue( r.ok === false && isNaN(r.value) );
         r = WAVE.tryParseInt(-1);
         assertTrue( r.ok === true && r.value === -1 );
         r = WAVE.tryParseInt("-1");
         assertTrue( r.ok === true && r.value === -1 );
         r = WAVE.tryParseInt(-999933321);
         assertTrue( r.ok === true && r.value === -999933321 );
         r = WAVE.tryParseInt("999933321");
         assertTrue( r.ok === true && r.value === 999933321 );
       });

       run("Integers","tryParseInt_allowReal",function() {
         var r = WAVE.tryParseInt("9.89", true);
         assertTrue( r.ok && r.value === 9);

         r = WAVE.tryParseInt("-9.89", true);
         assertTrue( r.ok && r.value === -9);
         
         var r = WAVE.tryParseInt("9.89");
         assertFalse( r.ok );

         r = WAVE.tryParseInt("-9.89");
         assertFalse( r.ok ); 

         var r = WAVE.tryParseInt("9.001", true);
         assertTrue( r.ok && r.value === 9);

         r = WAVE.tryParseInt("-9.001", true);
         assertTrue( r.ok && r.value === -9);
       });

       run("Integers", "intValid", function(){
         assertTrue( WAVE.intValid(" 1 "));
         assertTrue( WAVE.intValid(1));
         assertTrue( WAVE.intValid("1"));
         assertTrue( WAVE.intValid("100000"));
         assertTrue( WAVE.intValid("-123"));
         assertTrue( WAVE.intValid(0000));
         assertTrue( WAVE.intValid("0000"));
         assertFalse( WAVE.intValid(null));
         assertFalse( WAVE.intValid(""));
         assertFalse( WAVE.intValid("1   3"));
         assertFalse( WAVE.intValid("aaa100"));
         assertFalse( WAVE.intValid("001aaa"));
         assertFalse( WAVE.intValid("123kaka"));
         assertFalse( WAVE.intValid("1.2"));
         assertFalse( WAVE.intValid("0,2"));
         assertFalse( WAVE.intValid("0.2"));
         assertFalse( WAVE.intValid(".2"));
         assertFalse( WAVE.intValid("some text"));
       });

       run("Integers", "intValidPositive", function(){
         assertTrue(  WAVE.intValidPositive(11));
         assertFalse( WAVE.intValidPositive(0));
         assertFalse( WAVE.intValidPositive(-11));
         assertTrue(  WAVE.intValidPositive("1"));
         assertTrue(  WAVE.intValidPositive("100000"));
         assertFalse( WAVE.intValidPositive("-123"));
         assertFalse( WAVE.intValidPositive(null));
         assertFalse( WAVE.intValidPositive(""));
         assertFalse( WAVE.intValidPositive("some text"));
       });

       run("Integers", "intValidPositiveOrZero", function(){
         assertTrue(  WAVE.intValidPositiveOrZero(11));
         assertTrue(  WAVE.intValidPositiveOrZero(0));
         assertFalse( WAVE.intValidPositiveOrZero(-11));
         assertTrue(  WAVE.intValidPositiveOrZero("1"));
         assertTrue(  WAVE.intValidPositiveOrZero("100000"));
         assertFalse( WAVE.intValidPositiveOrZero("-123"));
         assertFalse( WAVE.intValidPositiveOrZero(null));
         assertFalse( WAVE.intValidPositiveOrZero(""));
         assertFalse( WAVE.intValidPositiveOrZero("some text"));
       });
      

       run("Formatting", "formatMoneyBasic", function(){
           assertTrue( "125.08" === WAVE.formatMoney(125.0890));
           assertTrue( "1,256.08" === WAVE.formatMoney(1256.0890));
       });
        
       run("Formatting", "formatMoneyDiffSeparators", function(){
           assertTrue( "1.256,08" === WAVE.formatMoney(1256.0890, ',', '.'));
           assertTrue( "1,256:::08" === WAVE.formatMoney(1256.0890, ':::'));
       });
        
       run("Formatting", "formatMoneyNegative", function(){
           assertTrue( "-1,256.08" === WAVE.formatMoney(-1256.0890));
           assertTrue( "-1,256.09" === WAVE.formatMoney(-1256.0999));
           assertTrue( "1,256.09" === WAVE.formatMoney(1256.0999));

           assertTrue( "16,345,256.41" === WAVE.formatMoney(16345256.41945));
           assertTrue( "-16,345,256.41" === WAVE.formatMoney(-16345256.41945));
       });
        
       run("Formatting", "formatMoneyNegativeDiffSeparators", function(){
           assertTrue( "16.345.256`41" === WAVE.formatMoney(16345256.41945, '`', '.'));
           assertTrue( "-16.345.256`41" === WAVE.formatMoney(-16345256.41945, '`', '.'));
       });



       run("Strings", "strEmpty", function(){
           assertTrue( WAVE.strEmpty(), "undefined1" );
           assertTrue( WAVE.strEmpty(undefined), "undefined2" );
           assertTrue( WAVE.strEmpty(null), "null" );
           assertTrue( WAVE.strEmpty(""),   "empty string" );
           assertTrue( WAVE.strEmpty("  "), "string with blanks" );
           assertFalse( WAVE.strEmpty(" a "), "string with values and blanks" );
       });

       run("Strings", "strAsBool", function(){
           assertFalse( WAVE.strAsBool(), "undefined" );
           assertTrue( WAVE.strAsBool(undefined, true), "undefined with true default" );
           assertFalse( WAVE.strAsBool(null), "null" );
           assertFalse( WAVE.strAsBool(""), "empty" );
           assertFalse( WAVE.strAsBool("  "), "spaces" );
           assertFalse( WAVE.strAsBool("false"), "false" );
           assertTrue( WAVE.strAsBool("true"), "true from lower case" );
           assertTrue( WAVE.strAsBool("True"), "True from upper case" );
           assertTrue( WAVE.strAsBool("1"), "1 char" );
           assertTrue( WAVE.strAsBool(" 1 "), "1 char with spaces" );
           assertFalse( WAVE.strAsBool("0"), "0 char" );
           assertTrue( WAVE.strAsBool(1), "1 int" );
           assertFalse( WAVE.strAsBool(0), "0 int" );
           assertTrue( WAVE.strAsBool(" True  "), "True+spaces" );
           assertTrue( WAVE.strAsBool(" True  "), "True+spaces" );

           assertTrue ( WAVE.strAsBool(true), "Bool True" );
           assertFalse( WAVE.strAsBool(false), "Bool False" );
       });

       run("Strings", "strDefault", function(){
           var NONE;
           assertTrue( 'something'===WAVE.strDefault('something')  );
        log("a");
        log("On vernul: "+WAVE.strDefault(null));

           assertTrue( ''===WAVE.strDefault(null)  );
        log("b");
           assertTrue( ''===WAVE.strDefault(NONE)  );
        log("c");
       });

       run("Strings", "strDefaultWithDefault", function(NOT_EXISTING){
           var NONE;
           assertTrue( 'something'===WAVE.strDefault('something','nnn')  );
           assertTrue( 'abc'===WAVE.strDefault(null, 'abc')  );
           assertTrue( 'def'===WAVE.strDefault(NONE, 'def')  );
       });

       run("Strings", "strDefaultObject", function(){
           var obj = {a: "aa", b: "bb"};
           assertTrue( 'aa'===WAVE.strDefault(obj['a'], 'abc')  );
           assertTrue( 'bb'===WAVE.strDefault(obj.b, 'abc')  );
           assertTrue( 'abc'===WAVE.strDefault(obj['c'], 'abc')  );
           assertTrue( 'def'===WAVE.strDefault(obj.d, 'def')  );
       });

       run("Strings", "strDefault With Empty", function(){
           assertTrue("     " === WAVE.strDefault("     ", "zina"));
       });

       run("Strings", "strEmptyDefault", function(){
           assertTrue("zina" === WAVE.strEmptyDefault("     ", "zina"), 1);
           assertTrue("zina" === WAVE.strEmptyDefault(undefined, "zina"), 2);
           assertTrue("zina" === WAVE.strEmptyDefault(null, "zina"), 3);
           assertTrue("zina" === WAVE.strEmptyDefault("", "zina"), 4);
           assertTrue("kukushka" === WAVE.strEmptyDefault("kukushka", "zina"), 5);
       });

       run("Strings", "nlsNameDefault", function(){
           var nls;
           assertTrue( ''===WAVE.nlsNameDefault() );
           assertTrue( ''===WAVE.nlsNameDefault(nls) );
           assertTrue( 'aaa'===WAVE.nlsNameDefault(nls, 'aaa') );

           nls = {n: "hello"};
           assertTrue( 'hello'===WAVE.nlsNameDefault(nls) );
           assertTrue( 'hello'===WAVE.nlsNameDefault(nls, 'aaa') );
           nls.n = null;
           assertTrue( 'aaa'===WAVE.nlsNameDefault(nls, 'aaa') );
           nls = {};
           assertTrue( 'aaa'===WAVE.nlsNameDefault(nls, 'aaa') );
       });

       run("Strings", "nlsDescrDefault", function(){
           var nls;
           assertTrue( ''===WAVE.nlsDescrDefault() );
           assertTrue( ''===WAVE.nlsDescrDefault(nls) );
           assertTrue( 'aaa'===WAVE.nlsDescrDefault(nls, 'aaa') );

           nls = {d: "hello"};
           assertTrue( 'hello'===WAVE.nlsDescrDefault(nls) );
           assertTrue( 'hello'===WAVE.nlsDescrDefault(nls, 'aaa') );
           nls.d = null;
           assertTrue( 'aaa'===WAVE.nlsDescrDefault(nls, 'aaa') );
           nls = {};
           assertTrue( 'aaa'===WAVE.nlsDescrDefault(nls, 'aaa') );
       });

       run("Strings", "nlsNameOrDescrDefault", function(){
           var nls;
           assertTrue( ''===WAVE.nlsNameOrDescrDefault() );
           assertTrue( ''===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'aaa'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );

           nls = {n: "myname"};
           assertTrue( 'myname'===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'myname'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );

           nls = {n: "hahaname", d: "mydescr"};
           assertTrue( 'hahaname'===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'hahaname'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );

            nls = {n: "", d: "mydescr"};
           assertTrue( 'mydescr'===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'mydescr'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );

           nls = {n: "    ", d: "mydescr"};
           assertTrue( 'mydescr'===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'mydescr'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );
           
           nls.n = "againname";
           assertTrue( 'againname'===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'againname'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );

           nls.n = null;
           assertTrue( 'mydescr'===WAVE.nlsNameOrDescrDefault(nls) );
           assertTrue( 'mydescr'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );

           nls.d = null;
           assertTrue( 'aaa'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );
           nls = {};
           assertTrue( 'aaa'===WAVE.nlsNameOrDescrDefault(nls, 'aaa') );
       });

       run("Strings", "nlsDescrOrNameDefault", function(){
           var nls;
           assertTrue( ''===WAVE.nlsDescrOrNameDefault() );
           assertTrue( ''===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'aaa'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );

           nls = {d: "mydescr"};
           assertTrue( 'mydescr'===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'mydescr'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );

           nls = {n: "hahaname", d: "mydescr"};
           assertTrue( 'mydescr'===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'mydescr'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );

           nls = {n: "zzzz", d: ""};
           assertTrue( 'zzzz'===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'zzzz'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );

           nls = {n: "zzzz", d: "   "};
           assertTrue( 'zzzz'===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'zzzz'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );
           
           nls.d = "againd";
           assertTrue( 'againd'===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'againd'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );

           nls.d = null;
           assertTrue( 'zzzz'===WAVE.nlsDescrOrNameDefault(nls) );
           assertTrue( 'zzzz'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );

           nls.n = null;
           assertTrue( 'aaa'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );
           nls = {};
           assertTrue( 'aaa'===WAVE.nlsDescrOrNameDefault(nls, 'aaa') );
       });




       run("Strings", "strSame", function(){
           assertTrue( WAVE.strSame("a","A"));
           assertTrue( WAVE.strSame("A","A"));
           assertTrue( WAVE.strSame("A","a"));
           assertTrue( WAVE.strSame("A","A "));
           assertTrue( WAVE.strSame(" A ","A"));
           assertTrue( WAVE.strSame("A BCD","   A bcd"));
           assertFalse( WAVE.strSame("A BCD","   A b cd"));
           assertFalse( WAVE.strSame("AB","A B"));
       });

       run("Strings", "strOneOf", function(){
           assertTrue( WAVE.strOneOf("car",["car","truck","suv"]) );
           assertTrue( WAVE.strOneOf("cAr",["car","truck","suv"]) );
           assertTrue( WAVE.strOneOf(" cAr ",["car","truck","suv"]) );
           assertTrue( WAVE.strOneOf("cAr",["cAR","truck","suv"]) );

           assertTrue( WAVE.strOneOf("trUCK",["cAR","truck","suv"]) );

           assertFalse( WAVE.strOneOf("Lenin",["cAR","truck","suv"]) );
           assertFalse( WAVE.strOneOf("Apple",["cAR","truck","suv"]) );

           assertTrue( WAVE.strOneOf("Apple|Orange",["apple","orange","apple|orange"],';') );
           assertFalse( WAVE.strOneOf("Apple|Orange",["apple","orange","orange|apple"],';') );
       });



       run("Strings", "strContains", function(){
           assertTrue( WAVE.strContains("Hello to all!","ALL"));
           assertFalse( WAVE.strContains("Hello to all!","ALL", true));
           assertTrue( WAVE.strContains("Hello to all!","all", true));

           assertTrue( WAVE.strContains("Hello to all!","HELLO"));
           assertFalse( WAVE.strContains("Hello to all!","HELLO", true));
           assertTrue( WAVE.strContains("Hello to all!","Hello", true));
       });

       run("Strings", "strTrim", function(){
           assertTrue( "aa" === WAVE.strTrim("    aa  "));
           assertTrue( "aa" === WAVE.strTrim("aa  "));
           assertTrue( "aa" === WAVE.strTrim("    aa"));
       });

       run("Strings", "strLTrim", function(){
           assertTrue( "aa  " === WAVE.strLTrim("    aa  "));
           assertTrue( "aa  " === WAVE.strLTrim("aa  "));
           assertTrue( "aa" === WAVE.strLTrim("    aa"));
       });

       run("Strings", "strRTrim", function(){
           assertTrue( "    aa" === WAVE.strRTrim("    aa  "));
           assertTrue( "aa" === WAVE.strRTrim("aa  "));
           assertTrue( "    aa" === WAVE.strRTrim("    aa"));
       });

       run("Strings", "strTrunc", function(){
           assertTrue( "Naming convention" === WAVE.strTrunc("Naming convention", 20));
           assertTrue( "Naming convention" === WAVE.strTrunc("Naming convention", 17));
           assertTrue( "Naming conven..." === WAVE.strTrunc("Naming convention", 16));
           assertTrue( "Naming c**" === WAVE.strTrunc("Naming convention", 10, "**"));
       });

       run("Strings", "strCaps", function(){
           assertTrue( "Alex Josephy" === WAVE.strCaps("alex Josephy"));
           assertTrue( "Alex Josephy" === WAVE.strCaps("Alex Josephy"));
           assertTrue( "Alex Josephy" === WAVE.strCaps("alex josephy"));
           assertTrue( "Alexjosephy"  === WAVE.strCaps("alexjosephy"));
           assertTrue( "Alexjosephy"  === WAVE.strCaps("Alexjosephy"));
       });

       run("Strings", "strStartsWith-insensitive", function(){
           assertTrue( WAVE.strStartsWith("abc","A") );
           assertFalse( WAVE.strStartsWith("abc","b") );

           assertTrue( WAVE.strStartsWith("abcdef","aBC") );
           assertFalse( WAVE.strStartsWith("abcdef","abd") );
       });

        run("Strings", "strStartsWith-sensitive", function(){
           assertTrue( WAVE.strStartsWith("abc","a",true) );
           assertFalse( WAVE.strStartsWith("abc","A",true) );
           assertFalse( WAVE.strStartsWith("abc","b",true) );

           assertTrue( WAVE.strStartsWith("abcdef","abc",true) );
           assertFalse( WAVE.strStartsWith("abcdef","abd",true) );
       });

       run("Strings", "strEndsWith-insensitive", function(){
           assertTrue( WAVE.strEndsWith("abc","C") );
           assertFalse( WAVE.strEndsWith("abc","b") );

           assertTrue( WAVE.strEndsWith("abcdef","DEf") );
           assertFalse( WAVE.strEndsWith("abcdef","aef") );
       });

       run("Strings", "strEndsWith-sensitive", function(){
           assertTrue( WAVE.strEndsWith("abc","c",true) );
           assertFalse( WAVE.strEndsWith("abc","C",true) );
           assertFalse( WAVE.strEndsWith("abc","b",true) );

           assertTrue( WAVE.strEndsWith("abcdef","def",true) );
           assertFalse( WAVE.strEndsWith("abcdef","aef",true) );
       });


       run("Strings", "strEnsureEnding", function(){
           assertTrue( "abc/" === WAVE.strEnsureEnding("abc","/") );
           assertTrue( "abc/" === WAVE.strEnsureEnding("abc/","/") );
           assertTrue( "abc/def" === WAVE.strEnsureEnding("abc","/def") );
           assertTrue( "abc/def" === WAVE.strEnsureEnding("abc/def","/def") );
       });

       run("Strings", "strEscapeHTML-1", function(){
           assertTrue( "A &lt; B?" === WAVE.strEscapeHTML("A < B?") );
           assertTrue( "A &gt; B?" === WAVE.strEscapeHTML("A > B?") );
           assertTrue( "A &amp; B?" === WAVE.strEscapeHTML("A & B?") );
           assertTrue( "&amp;&lt;&gt;" === WAVE.strEscapeHTML("&<>") );
       });

       run("Strings", "strEscapeHTML-2", function(){
           assertTrue( "A = &#39;yes&#39;" === WAVE.strEscapeHTML("A = 'yes'") );
           assertTrue( "A = &quot;yes&quot;" === WAVE.strEscapeHTML('A = "yes"') );
       });

       run("Strings", "strHTMLTemplate", function(){
           var result = WAVE.strHTMLTemplate("<td>@Name@</td>",{Name: "Mc'Loud"});
           log( WAVE.strEscapeHTML(result));
           assertTrue( "<td>Mc&#39;Loud</td>" === result );
       });

       run("Strings", "strTemplate", function(){
           var result = WAVE.strTemplate("{name: '@Name@'}",{Name: 'Oleg "The Hammer"'});
           log(result);
           assertTrue( "{name: 'Oleg \"The Hammer\"'}" === result );
       });

       run("Strings", "strTemplateFun", function(){
           var result = WAVE.strTemplateFun("{name: '@A@ or @B@'}", function(s, key){ return key==="A" ? "AZZ" : "BZZ"; });
           log(result);
           assertTrue( "{name: 'AZZ or BZZ'}" === result );
       });

       run("Strings", "strHTMLTemplateFun", function(){
           var result = WAVE.strHTMLTemplateFun("{name: '@A@ or @B@'}", function(s, key){ return key==="A" ? "A<1" : "B>2"; });
           log(result);
           assertTrue( "{name: 'A&lt;1 or B&gt;2'}" === result );
       });


        run("Strings", "strIsEmail", function(){
          
           assertTrue( WAVE.strIsEMail("a@bc.de") );
           assertTrue( WAVE.strIsEMail("aaa@baaac.daaaae") );
           assertTrue( WAVE.strIsEMail("aron.borisov@do.notexist.com") );
           assertFalse( WAVE.strIsEMail("@do.notexist.com") );
           assertFalse( WAVE.strIsEMail("aron@do") );
           assertFalse( WAVE.strIsEMail("aron@.do") );
           assertFalse( WAVE.strIsEMail("ar.on@do") );
           assertFalse( WAVE.strIsEMail("ar@d.x") );
           assertFalse( WAVE.strIsEMail("aaaa@.zzz") );
           assertFalse( WAVE.strIsEMail("") );
           assertFalse( WAVE.strIsEMail(null) );
           assertFalse( WAVE.strIsEMail("aaa@ba@aac.daaaae") );
           assertFalse( WAVE.strIsEMail("aaa$ba@aac.daaaae") );
           assertFalse( WAVE.strIsEMail("aaa ba@aac.daaaae") );
           assertFalse( WAVE.strIsEMail("aaa->ba@aac.daaaae") );
           assertTrue( WAVE.strIsEMail("aaa-b.a@aac.daaaae") );
           assertTrue( WAVE.strIsEMail("non.existent1945@gmale.kom") );
           assertTrue( WAVE.strIsEMail("non-existent1945@gmale.kom") );
           assertTrue( WAVE.strIsEMail("1945non-existent@gmale.kom") );
       });


       run("Strings", "strIsScreenName", function(){
          assertFalse( WAVE.strIsScreenName ("10o") );
          assertFalse( WAVE.strIsScreenName ("1.0o") );
          assertFalse( WAVE.strIsScreenName (".aa") );
          assertFalse( WAVE.strIsScreenName ("2d-2222") );
          assertFalse( WAVE.strIsScreenName ("DIMA-aaaaa..") );
          assertFalse( WAVE.strIsScreenName ("дима 123") );
          assertFalse( WAVE.strIsScreenName (".дима 123") );
          assertFalse( WAVE.strIsScreenName ("1дима-123") );
          assertFalse( WAVE.strIsScreenName ("-дима") );
          assertFalse( WAVE.strIsScreenName ("дима.") );


          assertTrue( WAVE.strIsScreenName("dima-qwerty") );
          assertTrue( WAVE.strIsScreenName("d2-2222") );
          assertTrue( WAVE.strIsScreenName("дима123") );
          assertTrue( WAVE.strIsScreenName("дима-123") );
          assertTrue( WAVE.strIsScreenName("дима.123") );
       });

       run("Strings", "siNum 1 <= n < 1000", function () {
         assertTrue("1.00" === WAVE.siNum(1), "1");
         assertTrue("1" === WAVE.siNum(1, 0), "2");
         assertTrue("999.0" === WAVE.siNum(999, 1), "3");
         assertTrue("1.0k" === WAVE.siNum(999.99, 1), "4");
         assertTrue("900.0k" === WAVE.siNum(900000, 1), "5");
         assertTrue("1M" === WAVE.siNum(999999.9, 0), "6");

         assertTrue("23.000" === WAVE.siNum(23, 3));
         assertTrue("-100.0" === WAVE.siNum(-99.99, 1));
       });

       run("Strings", "siNum 0 <= n < 1", function () {
         assertTrue("0.00" === WAVE.siNum(0));
         assertTrue("100.00m" === WAVE.siNum(0.1));
         assertTrue("-10.010m" === WAVE.siNum(-0.01001, 3));
         assertTrue("1.2m" === WAVE.siNum(0.00118, 1));
         assertTrue("10µ" === WAVE.siNum(0.00001, 0));
       });

        run("Strings", "charIsAZLetterOrDigit", function(){
          assertTrue( WAVE.charIsAZLetterOrDigit("a") );
          assertTrue( WAVE.charIsAZLetterOrDigit("d") );
          assertTrue( WAVE.charIsAZLetterOrDigit("z") );
          assertTrue( WAVE.charIsAZLetterOrDigit("A") );
          assertTrue( WAVE.charIsAZLetterOrDigit("D") );
          assertTrue( WAVE.charIsAZLetterOrDigit("Z") );
          assertTrue( WAVE.charIsAZLetterOrDigit("0") );
          assertTrue( WAVE.charIsAZLetterOrDigit("9") );
          assertTrue( WAVE.charIsAZLetterOrDigit("3") );

          assertFalse( WAVE.charIsAZLetterOrDigit("") );
          assertFalse( WAVE.charIsAZLetterOrDigit(" ") );
          assertFalse( WAVE.charIsAZLetterOrDigit(".") );
          assertFalse( WAVE.charIsAZLetterOrDigit(",") );
          assertFalse( WAVE.charIsAZLetterOrDigit("#") );
       });

       run("Strings", "strNormalizeUSPhone1", function(){
          var n = WAVE.strNormalizeUSPhone("5552223344");
          log(n);
          assertTrue( "(555) 222-3344" === n );
       });

       run("Strings", "strNormalizeUSPhone2", function(){
          var n = WAVE.strNormalizeUSPhone("2224415");
          log(n);
          assertTrue( "(???) 222-4415" === n );
       });

       run("Strings", "strNormalizeUSPhone3", function(){
          var n = WAVE.strNormalizeUSPhone("   +38 067 2148899   ");
          log(n);
          assertTrue( "+38 067 2148899" === n );
       });

       run("Strings", "strNormalizeUSPhone4", function(){
          var n = WAVE.strNormalizeUSPhone("555-222-4415");
          log(n);
          assertTrue( "(555) 222-4415" === n );
       });

       run("Strings", "strNormalizeUSPhone5", function(){
          var n = WAVE.strNormalizeUSPhone("555-222-4415 EXT 2014");
          log(n);
          assertTrue( "(555) 222-4415x2014" === n );
       });

       run("Strings", "strNormalizeUSPhone6", function(){
          var n = WAVE.strNormalizeUSPhone("555-222-4415.2014");
          log(n);
          assertTrue( "(555) 222-4415x2014" === n );
       });

       run("Strings", "strNormalizeUSPhone7", function(){
          var n = WAVE.strNormalizeUSPhone("555-222-4415EXT.2014");
          log(n);
          assertTrue( "(555) 222-4415x2014" === n );
       });

       run("Strings", "strNormalizeUSPhone8", function(){
          var n = WAVE.strNormalizeUSPhone("555-222-4415 X 2014");
          log(n);
          assertTrue( "(555) 222-4415x2014" === n );
       });

       run("Strings", "strNormalizeUSPhone9", function(){
          var n = WAVE.strNormalizeUSPhone("555.222.4415");
          log(n);
          assertTrue( "(555) 222-4415" === n );
       });

       run("Strings", "strNormalizeUSPhone10", function(){
          var n = WAVE.strNormalizeUSPhone("555-222-4415");
          log(n);
          assertTrue( "(555) 222-4415" === n );
       });

       run("Strings", "strNormalizeUSPhone11", function(){
          var n = WAVE.strNormalizeUSPhone("5552224415ext123");
          log(n);
          assertTrue( "(555) 222-4415x123" === n );
       });

       run("Strings", "strNormalizeUSPhone12", function(){
          var n = WAVE.strNormalizeUSPhone("5552224415ext.123");
          log(n);
          assertTrue( "(555) 222-4415x123" === n );
       });



       run("Conversion", "str->int", function(){
         var v = WAVE.convertScalarType(false, "1", "int");
         assertTrue( v === 1 );
         assertTrue( WAVE.intValid(v));
       });

       run("Conversion", "negative str->int", function(){
         var v = WAVE.convertScalarType(false, "-181", "int");
         assertTrue( v === -181 );
         assertTrue( WAVE.intValid(v));
       });

       run("Conversion", "nullable str->int", function(){
         var v = WAVE.convertScalarType(false, "1", "int");
         assertTrue( v === 1 );
         assertTrue( WAVE.intValidPositiveOrZero(v));

         v = WAVE.convertScalarType(false, null, "int");
         assertTrue( v === 0 );
         assertTrue( WAVE.intValidPositiveOrZero(v));

         v = WAVE.convertScalarType(true, null, "int");
         assertTrue( v === null );

         v = WAVE.convertScalarType(true, "", "int");  //empty string
         assertTrue( v === null );
       });

       run("Conversion", "error str->int", function(){
         try
         {
           var v = WAVE.convertScalarType(false, "zizikaka", "int");
           assertTrue( false );
         }
         catch(error)
         {
            log(error);
            assertTrue( true );
         }
       });

       run("Conversion", "default str->int", function(){
          var v = WAVE.convertScalarType(false, "zizikaka", "int", 190);
          assertTrue( 190 === v );
       });

       run("Conversion", "bool->int", function(){
          assertTrue( 1 === WAVE.convertScalarType(false, true, "int") );
          assertTrue( 0 === WAVE.convertScalarType(false, false, "int") );
       });

       run("Conversion", "real->int", function(){
          assertTrue( 1 === WAVE.convertScalarType(false, 1.1, "int"), "case 1" );
          assertTrue( -19 === WAVE.convertScalarType(false, -19.2, "int"), "case 2" );
       });

       run("Conversion", "date->int", function(){
          var v =  WAVE.convertScalarType(false, new Date(1980, 07, 02, 8, 00, 00, 00), "int");
          log(v);
          assertTrue(334040400000 === v);
       });

       run("Conversion", "null->str", function(){
          assertTrue( "" === WAVE.convertScalarType(false, null, "string") );
       });

       run("Conversion", "int->str", function(){
          assertTrue( "10" === WAVE.convertScalarType(false, 10, "string") );
          assertTrue( "0" === WAVE.convertScalarType(false, 0, "string") );
          assertTrue( "-10" === WAVE.convertScalarType(false, -10, "string") );
       });

       run("Conversion", "bool->str", function(){
          assertTrue( "true" === WAVE.convertScalarType(false, true, "string") );
          assertTrue( "false" === WAVE.convertScalarType(false, false, "string") );
       });

       run("Conversion", "real->str", function(){
          assertTrue( "122.23" === WAVE.convertScalarType(false, 122.23, "string") );
          assertTrue( "-270.011" === WAVE.convertScalarType(false, -270.011, "string") );
       });

       run("Conversion", "date->str", function(){
          assertTrue( "03/15/2011 14:30:00" === WAVE.convertScalarType(false, new Date(2011, 02, 15, 14, 30, 00), "string") );
       });

       run("Conversion", "date->str->date", function(){
          var d = WAVE.convertScalarType(false, WAVE.convertScalarType(false, new Date(2011, 02, 15, 14, 30, 00), "string"), "date");
          assertTrue( 2011 === d.getFullYear());
          assertTrue( 02 === d.getMonth());
          assertTrue( 15 === d.getDate());
       });
       
       run("Conversion", "null->bool", function(){
          assertTrue( false === WAVE.convertScalarType(false, null, "bool") );
       });

       run("Conversion", "int->bool", function(){
          assertTrue( false === WAVE.convertScalarType(false, 0, "bool") );
          assertTrue( true === WAVE.convertScalarType(false, 1, "bool") );
          assertTrue( true === WAVE.convertScalarType(false, -10, "bool") );
       });

       run("Conversion", "real->bool", function(){
          assertTrue( false === WAVE.convertScalarType(false, 0.0, "bool") );
          assertTrue( true === WAVE.convertScalarType(false, 123.11, "bool") );
          assertTrue( true === WAVE.convertScalarType(false, -2.0, "bool") );
       });

       run("Conversion", "null->date", function(){
         try
         {
           WAVE.convertScalarType(false, null, "date");
           assertTrue( false );
         }
         catch(error)
         {
            log(error);
            assertTrue( true );
         }
       });

       run("Conversion", "null->date with dflt", function(){
          var d = WAVE.convertScalarType(false, null, "date", new Date(2012, 04, 12));
          assertTrue( d.getFullYear()===2012 );
          assertTrue( d.getMonth()===04 );
          assertTrue( d.getDate()===12 );
       });

       run("Conversion", "str->date", function(){
          var d = WAVE.convertScalarType(false, "02/15/2011", "date");
          log (d.toString());
          assertTrue( d.getFullYear()===2011 );
          assertTrue( d.getMonth()===01 );
          assertTrue( d.getDate()===15 );
       });

        run("Conversion", "bad date string->date", function(){
         try
         {
           WAVE.convertScalarType(false, "impossible date", "date");
           assertTrue( false );
         }
         catch(error)
         {
            log(error);
            assertTrue( true );
         }
       });

       run("Conversion", "bad date->date with dflt", function(){
          var d = WAVE.convertScalarType(false, "impossible date", "date", new Date(2012, 04, 12));
          assertTrue( d.getFullYear()===2012 );
          assertTrue( d.getMonth()===04 );
          assertTrue( d.getDate()===12 );
       });




       run("Date-Time", "toISODateTimeString", function(){
           log( WAVE.toISODateTimeString( new Date( Date.UTC(1980, 04, 07, 14, 15, 44, 00) ) ) );
           assertTrue( "1980-05-07T14:15:44Z" === WAVE.toISODateTimeString( new Date( Date.UTC(1980, 04, 07, 14, 15, 44, 00))) );
       });

       run("Date-Time", "toUSDateTimeString", function(){
           log( WAVE.toUSDateTimeString( new Date(1980, 04, 07, 14, 15, 44, 00) )  );
           assertTrue( "05/07/1980 14:15:44" === WAVE.toUSDateTimeString( new Date(1980, 04, 07, 14, 15, 44, 00)) );
       });

       run("Date-Time", "toUSDateString", function(){
           log( WAVE.toUSDateString( new Date(1980, 04, 07, 14, 15, 44, 00) )  );
           assertTrue( "05/07/1980" === WAVE.toUSDateString( new Date(1980, 04, 07, 14, 15, 44, 00)) );
       });

       run("Date-Time", "toSeconds", function(){
           assertTrue( 10 === WAVE.toSeconds("10s") );
           assertTrue( 70 === WAVE.toSeconds("1m 10s") );
       });

       run("Date-Time", "toSeconds-withHours", function(){
           assertTrue( 3670 === WAVE.toSeconds("1h 1m 10s") );
       });

       run("Date-Time", "toSeconds-withDays", function(){
           assertTrue( (3*24*60*60)+ (7*60*60) + (18*60) + 10 === WAVE.toSeconds("3d 7h 18m 10s") );
       });

       run("Key", "genRndKey-1", function(){
          for(var i=0; i<15; i++)
          {
           var key = WAVE.genRndKey(25, "azokuliABCDEF");
           logi( key ); 
           assertTrue( 25 === key.length); 
          }
       });

       run("Key", "genRndKey-dfltArgs", function(){
          for(var i=0; i<15; i++)
          {
           var key = WAVE.genRndKey();
           logi( key ); 
           assertTrue(8 === key.length); 
          }
       });


       run("Key", "genAutoincKey-CTR1", function(){
          var counter;
          for(var i=0; i<5; i++)
          {
           counter = WAVE.genAutoincKey("CTR1",5);
           logi( counter ); 
          }
          assertTrue( 20 === counter); 
       });


       run("Key", "genAutoincKey-dfltArgs", function(){
          var counter;
          for(var i=0; i<100; i++)
          {
           counter = WAVE.genAutoincKey();
           logi( counter ); 
          }
          assertTrue( 99 === counter); 
       });

       run("Random Generation", "rnd-defltArgs", function(){
          for(var i=0; i<50; i++)
          {
           var num = WAVE.rnd();
           logi( num ); 
           assertTrue( num >= 0 && num <= 100); 
          }
       });

       run("Random Generation", "rnd-upperBound", function(){
          for(var i=0; i<50; i++)
          {
           var num = WAVE.rnd(75);
           logi( num ); 
           assertTrue( num >= 0 && num <= 75); 
          }
       });

       run("Random Generation", "rnd-range", function(){
          for(var i=0; i<50; i++)
          {
           var num = WAVE.rnd(-10,10);
           logi( num ); 
           assertTrue( num >= -10 && num <= 10); 
          }
       });

       run("Cookies", "set+getCookie", function(){
          log("This test MAY FAIL if executed from local file system as cookies do not work properly in some browsers. Try running the page of 127.0.0.1 mapping");
          WAVE.setCookie("AAA", "123abc");
          assertTrue("123abc" === WAVE.getCookie("AAA"));
       });

       run("Cookies", "set+get+deleteCookie", function(){
          log("This test MAY FAIL if executed from local file system as cookies do not work properly in some browsers. Try running the page of 127.0.0.1 mapping");
          WAVE.setCookie("BBB", "67890");
          assertTrue("67890" === WAVE.getCookie("BBB"));
          WAVE.deleteCookie("BBB");
          assertFalse("67890" === WAVE.getCookie("BBB"));
       });


       run("Assertions", "lastLine", function(){
          log("This should be in the 'ASSERTIONS' area at page top, despite being declared at the very end of the test run");
       });


       run("Geometry", "PI", function(){
          assertTrue( 314 === Math.floor( 100 * WAVE.Geometry.PI));
       });

       run("Geometry", "PI2", function(){
          assertTrue( 628 === Math.floor( 100 * WAVE.Geometry.PI2));
       });

       run("Geometry", "MapDirection", function(){
          var dir1 = WAVE.Geometry.MapDirection.East;
          var dir2 = WAVE.Geometry.MapDirection.North;
          var dir3 = WAVE.Geometry.MapDirection.East;
          assertTrue( "East" === dir1.Name );
          assertTrue( "North" === dir2.Name );

          assertTrue( dir1 !== dir2);
          assertFalse( dir1 === dir2);
          assertTrue( dir1 === dir3);
       });

       run("Geometry", "distance", function(){
          assertTrue( 100 === WAVE.Geometry.distance(0,0, 100,0) );
          assertTrue( 100 === WAVE.Geometry.distance(0,0, 0, 100) );
          assertTrue( 141421 === Math.floor( 1000 *  WAVE.Geometry.distance(0,0, 100, 100) ) );
          assertTrue( 141421 === Math.floor( 1000 *  WAVE.Geometry.distance(100,0, 0, 100) ) );
          assertTrue( 141421 === Math.floor( 1000 *  WAVE.Geometry.distance(100,100, 0, 0) ) );
       });

       run("Geometry", "distancePoints", function(){
          var p = WAVE.Geometry.Point;

          assertTrue( 100 === WAVE.Geometry.distancePoints( new p(0,0), new p(100,0) ) );
          assertTrue( 100 === WAVE.Geometry.distancePoints( new p(0,0), new p(0, 100) ) );
          assertTrue( 141421 === Math.floor( 1000 *  WAVE.Geometry.distancePoints(new p(0,0)    ,new p(100, 100) ) ));
          assertTrue( 141421 === Math.floor( 1000 *  WAVE.Geometry.distancePoints(new p(100,0)  ,new p(0, 100) ) ));
          assertTrue( 141421 === Math.floor( 1000 *  WAVE.Geometry.distancePoints(new p(100,100),new p(0, 0) ) ));
       });


       run("Geometry", "radToDeg", function(){
          assertTrue( 180 === Math.floor( WAVE.Geometry.radToDeg( WAVE.Geometry.PI ) ));
          assertTrue( 360 === Math.floor( WAVE.Geometry.radToDeg( WAVE.Geometry.PI2 ) ));
       });

       run("Geometry", "degToRad", function(){
          assertTrue( 157 === Math.floor( WAVE.Geometry.degToRad( 90 ) * 100 ));
          assertTrue( 314 === Math.floor( WAVE.Geometry.degToRad( 180 ) * 100));
       });

       run("Geometry", "azimuthRad", function(){
         assertTrue(157 === Math.floor(WAVE.Geometry.azimuthRad(0, 0, 0, 100) * 100));
         assertTrue(0 === Math.floor( WAVE.Geometry.azimuthRad( 0,0, 100,0 ) * 100)); 
       });

       run("Geometry", "azimuthRadPoints", function(){
          var p = WAVE.Geometry.Point;

          assertTrue( 157 === Math.floor( WAVE.Geometry.azimuthRadPoints( new p(0,0) , new p(0, 100) ) * 100)); 
          assertTrue( 0 === Math.floor( WAVE.Geometry.azimuthRadPoints( new p(0,0) , new p(100,0) ) * 100)); 
       });

       run("Geometry", "azimuthDeg", function(){
          assertTrue( 90 === Math.round( WAVE.Geometry.azimuthDeg( 0,0, 0, 100 ) )); 
          assertTrue( 0 === Math.round( WAVE.Geometry.azimuthDeg( 0,0, 100,0 ) )); 
       });

       run("Geometry", "azimuthDegPoints", function(){
          var p = WAVE.Geometry.Point;

          assertTrue( 90 === Math.round( WAVE.Geometry.azimuthDegPoints(new p(0,0), new p(0,100) ) )); 
          assertTrue( 0 === Math.round( WAVE.Geometry.azimuthDegPoints( new p(0,0), new p(100,0) ) )); 
       });

       run("Geometry", "azimuthOfRadix", function () {
          assertTrue( 3 === WAVE.Geometry.azimuthOfRadix(0, 0, 0, -100, 4));
          assertTrue( 0 === WAVE.Geometry.azimuthOfRadix(0, 0, 100, -99, 4));
          assertTrue( 1 === WAVE.Geometry.azimuthOfRadix( 0, 0, 99,  100, 4 )  );
          assertTrue( 2 === WAVE.Geometry.azimuthOfRadix( 0, 0, -101, 100, 4 )  );
       });

       run("Geometry", "azimuthOfRadixPoints", function(){
          var p = WAVE.Geometry.Point;

          assertTrue( 3 === WAVE.Geometry.azimuthOfRadixPoints( new p(0, 0),new p(0, -100  ) ,  4 ) ); 
          assertTrue( 0 === WAVE.Geometry.azimuthOfRadixPoints( new p(0, 0),new p(100, -99) ,  4 ) );
          assertTrue( 1 === WAVE.Geometry.azimuthOfRadixPoints( new p(0, 0),new p(99, 100 ) ,  4 ) );
          assertTrue( 2 === WAVE.Geometry.azimuthOfRadixPoints( new p(0, 0),new p(-101, 100) ,  4 ) );
       });

       run("Geometry", "toRectXY-1", function(){
          var rect = WAVE.Geometry.toRectXY(0,0, 100, 75);

          assertTrue( 0   === rect.left() ); 
          assertTrue( 0   === rect.top() ); 
          assertTrue( 100 === rect.width() ); 
          assertTrue( 75  === rect.height() ); 
       });

       run("Geometry", "toRectXY-2", function(){
          var rect = WAVE.Geometry.toRectXY(0,0, -100, -75);

          assertTrue( -100   === rect.left() ); 
          assertTrue( -75   === rect.top() ); 
          assertTrue( 100 === rect.width() ); 
          assertTrue( 75  === rect.height() ); 
       });

       run("Geometry", "toRectWH-1", function(){
          var rect = WAVE.Geometry.toRectWH(0,0, 100, 75);

          assertTrue( 0   === rect.left() ); 
          assertTrue( 0   === rect.top() ); 
          assertTrue( 100 === rect.width() ); 
          assertTrue( 75  === rect.height() ); 
       });

       run("Geometry", "toRectXY-2", function(){
          var rect = WAVE.Geometry.toRectWH(0,0, -100, -75);

          assertTrue( -100   === rect.left() ); 
          assertTrue( -75   === rect.top() ); 
          assertTrue( 100 === rect.width() ); 
          assertTrue( 75  === rect.height() ); 
       });

       run("Geometry", "overlapAreaRect", function(){
          var rect = WAVE.Geometry.Rectangle;
          var area = WAVE.Geometry.overlapAreaRect( WAVE.Geometry.toRectWH(0,0, 100,100),
                                                    WAVE.Geometry.toRectWH(90,0, 100,100));  

          assertTrue( 10*100  === area); 
       });

       run("Geometry", "overlapAreaWH", function(){
          assertTrue( 10*100 ===  WAVE.Geometry.overlapAreaWH( 0, 0, 100, 100,      90,0, 100, 100) );
          assertTrue( 1*100 ===  WAVE.Geometry.overlapAreaWH(  0, 0, 100, 100,      99,0, 100, 100) );
          assertTrue( 70 * 100 === WAVE.Geometry.overlapAreaWH( -10, 0, 100, 100,  20,0, 100, 100) );
          assertTrue( 70 * 60 ===  WAVE.Geometry.overlapAreaWH(  -10, 0, 100, 100,  20, 40, 100, 100) );
          assertTrue( 10*100 ===  WAVE.Geometry.overlapAreaWH( 90,0, 100, 100,     0, 0, 100, 100 ) );
          assertTrue( 1*100 ===  WAVE.Geometry.overlapAreaWH(  99,0, 100, 100,    0, 0, 100, 100) );
          assertTrue( 70 * 100 === WAVE.Geometry.overlapAreaWH( 20,0, 100, 100,   -10, 0, 100, 100 ) );
          assertTrue( 70 * 60 ===  WAVE.Geometry.overlapAreaWH( 20, 40, 100, 100,  -10, 0, 100, 100) );
       });

       run("Geometry", "overlapAreaXY", function(){
          assertTrue( 10*100 ===   WAVE.Geometry.overlapAreaXY( 0, 0, 100, 100,      90,0, 100, 100) );
          assertTrue( 1*100 ===    WAVE.Geometry.overlapAreaXY(  0, 0, 100, 100,      99,0, 100, 100) );
          assertTrue( 70 * 100 === WAVE.Geometry.overlapAreaXY( -10, 0, 90, 100,  20,0, 100, 100) );
          assertTrue( 70 * 60 ===  WAVE.Geometry.overlapAreaXY(  -10, 0, 90, 100,  20, 40, 100, 100) );
          assertTrue( 10*100 ===   WAVE.Geometry.overlapAreaXY( 90,0, 190, 100,     0, 0, 100, 100 ) );
          assertTrue( 1*100 ===    WAVE.Geometry.overlapAreaXY(  99,0, 199, 100,    0, 0, 100, 100) );
          assertTrue( 70 * 100 === WAVE.Geometry.overlapAreaXY( 20,0, 120, 100,   -10, 0, 90, 100 ) );
          assertTrue( 70 * 60 ===  WAVE.Geometry.overlapAreaXY( 20, 40, 120, 140,  -10, 0, 90, 100) );
       });

       run("Geometry", "lineIntersectsRect", function(){
          assertFalse(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, -1, 0, -1, 3));
          assertFalse(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 5, 0, 5, 3));
          assertFalse(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 0, 4, 4, 4));
          assertFalse(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 0, -1, 4, -1));

          assertTrue(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 0, 0, 4, 0));
          assertTrue(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 4, 0, 4, 3));
          assertTrue(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 0, 3, 4, 3));
          assertTrue(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 0, 0, 4, 0));

          assertFalse(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 1, 5, -2, 2));
          assertTrue(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, 1, 5, -1, 1));
          assertTrue(WAVE.Geometry.lineIntersectsRect(0, 0, 4, 3, -1, -2, 6, 3));
       });

       run("Geometry", "intersectionLengthRectLineXY", function(){
          assertTrue(5 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, 0, 0, 4, 3));
          assertTrue(5 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, 0, 3, 4, 0));

          assertTrue(5 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, -1, -10, 5, 20));
          assertTrue(5 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, -10, 4, 6, -0.78));

          assertTrue(1 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, 2, 2, 2, 6));
          assertTrue(1 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, 2, 6, 2, 2));
          assertTrue(2 === WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, 2, 2, 6, 2));

          assertTrue(devOk(2 / Math.cos(Math.PI / 4), WAVE.Geometry.intersectionLengthRectLineXY(0, 0, 4, 3, 2, 1, 12, 4)));
       });

       run("Geometry", "wrapAngle", function(){
          assertTrue( 314 === Math.floor( 100 * WAVE.Geometry.wrapAngle( 0, WAVE.Geometry.PI * 3 )));
          assertTrue( 314 === Math.floor( 100 * WAVE.Geometry.wrapAngle( WAVE.Geometry.PI, WAVE.Geometry.PI * 2 ) ));
          assertTrue( 157 === Math.floor( 100 * WAVE.Geometry.wrapAngle( WAVE.Geometry.PI, - WAVE.Geometry.PI / 2 ) ));
          assertTrue( 314+157 === Math.floor( 100 * WAVE.Geometry.wrapAngle( WAVE.Geometry.PI, + WAVE.Geometry.PI / 2 ) ));
       });


       run("Geometry", "mapDirectionToAngle", function(){
          var dir = WAVE.Geometry.MapDirection;
          assertTrue( 157     === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.North)) );
          assertTrue( 314+157 === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.South)) );
          assertTrue( 0   === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.East)) );
          assertTrue( 314 === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.West)) );

          assertTrue( 157-79  === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.NorthEast)) );
          assertTrue( 157+78  === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.NorthWest)) );

          assertTrue( 314+157+78  === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.SouthEast)) );
          assertTrue( 314+157-79  === Math.floor(100 * WAVE.Geometry.mapDirectionToAngle(dir.SouthWest)) );
       });


       run("Geometry", "angleToMapDirection(mapDirectionToAngle)", function(){
          var dir = WAVE.Geometry.MapDirection;
          assertTrue( dir.North ===     WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.North)));
          assertTrue( dir.South ===     WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.South)));
          assertTrue( dir.East  ===     WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.East)));
          assertTrue( dir.West ===      WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.West)));

          assertTrue( dir.NorthEast === WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.NorthEast)));
          assertTrue( dir.NorthWest === WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.NorthWest)));

          assertTrue( dir.SouthEast === WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.SouthEast)));
          assertTrue( dir.SouthWest === WAVE.Geometry.angleToMapDirection(WAVE.Geometry.mapDirectionToAngle(dir.SouthWest)));
       });

       run("Geometry", "perimeterViolationArea", function(){
          var rwh = WAVE.Geometry.toRectWH;
          assertTrue( 0 ===      WAVE.Geometry.perimeterViolationArea( rwh(0,0,100,100), rwh(0,0,100,100)) );   
          assertTrue( 1*100 ===  WAVE.Geometry.perimeterViolationArea( rwh(0,0,100,100), rwh(1,0,100,100)) );
          assertTrue( 1*100 ===  WAVE.Geometry.perimeterViolationArea( rwh(0,0,100,100), rwh(-1,0,100,100)) );
          assertTrue( 20*100 === WAVE.Geometry.perimeterViolationArea( rwh(0,0,100,100), rwh(-10,-10,100,100)) );
       });

       run("Geometry", "findRayFromRectangleCenterSideIntersection-1", function(){
          var rwh = WAVE.Geometry.toRectWH;
          var pnt = WAVE.Geometry.findRayFromRectangleCenterSideIntersection( rwh(-50,-50,100,100), 0);   
          assertTrue( 50 === pnt.x()); 
          assertTrue( 0 === pnt.y()); 
       });

       run("Geometry", "findRayFromRectangleCenterSideIntersection-2", function(){
          var rwh = WAVE.Geometry.toRectWH;
          var pnt = WAVE.Geometry.findRayFromRectangleCenterSideIntersection( rwh(-50,-50,100,100), WAVE.Geometry.PI / 2);   
          log(pnt);
          assertTrue( 0 === Math.round(pnt.x())); 
          assertTrue( 50 === Math.round(pnt.y())); 
       });

       run("Geometry", "findRayFromRectangleCenterSideIntersection-3", function(){
          var rwh = WAVE.Geometry.toRectWH;
          var pnt = WAVE.Geometry.findRayFromRectangleCenterSideIntersection( rwh(-50,-50,100,100), WAVE.Geometry.PI);   
          log(pnt);
          assertTrue( -50 === Math.round(pnt.x())); 
          assertTrue( 0 === Math.round(pnt.y())); 
       });

       run("Geometry", "findRayFromRectangleCenterSideIntersection-4", function(){
          var rwh = WAVE.Geometry.toRectWH;
          var pnt = WAVE.Geometry.findRayFromRectangleCenterSideIntersection( rwh(-50,-50,100,100), WAVE.Geometry.PI+(WAVE.Geometry.PI / 2));   
          log(pnt);
          assertTrue( 0 === Math.round(pnt.x())); 
          assertTrue( -50 === Math.round(pnt.y())); 
       });

       run("Geometry", "getBBox-no-points", function(){
          var b = WAVE.Geometry.getBBox();   
          assertTrue(null === b);

          b = WAVE.Geometry.getBBox([]);   
          assertTrue(null === b);
       });

       run("Geometry", "getBBox-single-point", function(){
          var b = WAVE.Geometry.getBBox([{x: 0, y: 1}]);   

          assertTrue(0 === b.left());
          assertTrue(1 === b.top());
          assertTrue(0 === b.right());
          assertTrue(1 === b.bottom());
       });

       run("Geometry", "getBBox-regular", function(){
          var b = WAVE.Geometry.getBBox([{x: 0, y: 1}, {x: 20, y: -17}, {x: -15, y: 20.57}]);   

          assertTrue(-15 === b.left());
          assertTrue(-17 === b.top());
          assertTrue(20 === b.right());
          assertTrue(20.57 === b.bottom());
       });

       run("Geometry", "Point", function(){
          var pnt = new WAVE.Geometry.Point(100, 90);   
          assertTrue( 100 === pnt.x());
          assertTrue( 90 === pnt.y());
          log(pnt);
       });

       run("Geometry", "Point-set", function(){
          var pnt = new WAVE.Geometry.Point(100, 90);   
          pnt.x(110); 
          pnt.y(-98);
          assertTrue( 110 === pnt.x());
          assertTrue( -98 === pnt.y());
          log(pnt);
       });

       run("Geometry", "Point-offset", function(){
          var pnt = new WAVE.Geometry.Point(100, 90);   
          assertTrue( 100 === pnt.x());
          assertTrue( 90 === pnt.y());
          log(pnt);
          pnt.offset(12, -100);
          log(pnt);  
          assertTrue( 112 === pnt.x());
          assertTrue( -10 === pnt.y());
       });

       run("Geometry", "Point-isEqual", function(){
          var pnt1 = new WAVE.Geometry.Point(100, 90);
          var pnt2 = new WAVE.Geometry.Point(10, 17);
          var pnt3 = new WAVE.Geometry.Point(100, 90);
          assertFalse( pnt1.isEqual(pnt2));
          assertTrue ( pnt1.isEqual(pnt3));
       });

       run("Geometry", "Point-toPolarPoint", function(){
          var center = new WAVE.Geometry.Point(0, 0);
          var pnt = new WAVE.Geometry.Point(150, 0);
          
          var pp = pnt.toPolarPoint(center);
          assertTrue( 150 === pp.radius());
          assertTrue( 0 === pp.theta());
          log( pnt + " -> "+ pp);
       });

       run("Geometry", "PolarPoint-invalid", function(){
         try
         {
          var pp = new WAVE.Geometry.PolarPoint(100, 789);
         }
         catch(err)
         {
            log("Exception: " + err);
            assertTrue( WAVE.strContains(err, "angle") );
         }
       });

       run("Geometry", "PolarPoint-radius", function(){
          var pp = new WAVE.Geometry.PolarPoint(100, WAVE.Geometry.PI);
          assertTrue(100 === pp.radius()); 
          pp.radius(125);
          assertTrue(125 === pp.radius()); 
       });

       run("Geometry", "PolarPoint-theta", function(){
          var pp = new WAVE.Geometry.PolarPoint(100, WAVE.Geometry.PI);
          assertTrue(314 === Math.floor(100 * pp.theta())); 
          pp.theta(1.18);
          assertTrue(118 === Math.floor(100 * pp.theta()));  
       });

       run("Geometry", "PolarPoint-toPoint", function(){
          var pp = new WAVE.Geometry.PolarPoint(100, WAVE.Geometry.PI / 4);
          var p = pp.toPoint();
          assertTrue( 70 === Math.floor( p.x()));
          assertTrue( 70 === Math.floor( p.y()));
       });

       run("Geometry", "PolarPoint-isEqual", function(){
          var pnt1 = new WAVE.Geometry.PolarPoint(100, 1.2);
          var pnt2 = new WAVE.Geometry.PolarPoint(10, 2.17);
          var pnt3 = new WAVE.Geometry.PolarPoint(100, 1.2);
          assertFalse( pnt1.isEqual(pnt2));
          assertTrue ( pnt1.isEqual(pnt3));
       });

       run("Geometry", "PolarPoint-toString", function(){
          var pnt = new WAVE.Geometry.PolarPoint(100, WAVE.Geometry.PI);
          log( pnt );
          assertTrue("(100 , 180°)" === pnt.toString());
       });

       run("Geometry", "Rectangle-1", function(){
          var rect = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(0,0), new WAVE.Geometry.Point(100,100)); 
          log( rect );
          assertTrue( 0 === rect.left());
          assertTrue( 0 === rect.top());
          assertTrue( 100 === rect.right());
          assertTrue( 100 === rect.bottom());
          assertTrue( 100 === rect.width());
          assertTrue( 100 === rect.height());
          assertTrue( 0 === rect.topLeft().x());
          assertTrue( 0 === rect.topLeft().y());
          assertTrue( 100 === rect.bottomRight().x());
          assertTrue( 100 === rect.bottomRight().y());
       });

       run("Geometry", "Rectangle-2", function(){
          var rect = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(-50,-50), new WAVE.Geometry.Point(50,50)); 
          log( rect );
          assertTrue( -50 === rect.left());
          assertTrue( -50 === rect.top());
          assertTrue( 50 === rect.right());
          assertTrue( 50 === rect.bottom());
          assertTrue( 100 === rect.width());
          assertTrue( 100 === rect.height());
          assertTrue( -50 === rect.topLeft().x());
          assertTrue( -50 === rect.topLeft().y());
          assertTrue( 50 === rect.bottomRight().x());
          assertTrue( 50 === rect.bottomRight().y());
       });


       run("Geometry", "Rectangle-3", function(){
          var rect = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(50,50), new WAVE.Geometry.Point(-50,-50)); //note that corners are reversed
          log( rect );
          assertTrue( -50 === rect.left());
          assertTrue( -50 === rect.top());
          assertTrue( 50 === rect.right());
          assertTrue( 50 === rect.bottom());
          assertTrue( 100 === rect.width());
          assertTrue( 100 === rect.height());
          assertTrue( -50 === rect.topLeft().x());
          assertTrue( -50 === rect.topLeft().y());
          assertTrue( 50 === rect.bottomRight().x());
          assertTrue( 50 === rect.bottomRight().y());
       });


       run("Geometry", "Rectangle-4", function(){
          var rect = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(50,-50), new WAVE.Geometry.Point(-50,50)); //note that corners are reversed
          log( rect );
          assertTrue( -50 === rect.left());
          assertTrue( -50 === rect.top());
          assertTrue( 50 === rect.right());
          assertTrue( 50 === rect.bottom());
          assertTrue( 100 === rect.width());
          assertTrue( 100 === rect.height());
          assertTrue( -50 === rect.topLeft().x());
          assertTrue( -50 === rect.topLeft().y());
          assertTrue( 50 === rect.bottomRight().x());
          assertTrue( 50 === rect.bottomRight().y());
       });

       run("Geometry", "Rectangle-contains", function () {
         var rect = WAVE.Geometry.toRectXY(4, 0, 0, 2);

         assertTrue(rect.contains(new WAVE.Geometry.Point(0, 0)));
         assertTrue(rect.contains(new WAVE.Geometry.Point(4, 2)));
         assertTrue(rect.contains(new WAVE.Geometry.Point(0, 2)));
         assertTrue(rect.contains(new WAVE.Geometry.Point(4, 0)));
         assertTrue(rect.contains(new WAVE.Geometry.Point(2, 1)));

         assertFalse(rect.contains(new WAVE.Geometry.Point(-1, 1)));
         assertFalse(rect.contains(new WAVE.Geometry.Point(2, -1)));
         assertFalse(rect.contains(new WAVE.Geometry.Point(5, 1)));
         assertFalse(rect.contains(new WAVE.Geometry.Point(2, 2.1)));
       });

        run("Geometry", "Rectangle-isEqual", function(){
          var rect1 = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(0,0), new WAVE.Geometry.Point(100,100)); 
          var rect2 = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(-50,-50), new WAVE.Geometry.Point(50,50)); 
          var rect3 = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(100,100), new WAVE.Geometry.Point(0,0)); 
          log( rect1 );
          log( rect2 );
          log( rect3 );
          assertTrue( rect1.isEqual(rect3));
          assertFalse( rect1.isEqual(rect2));
       });


       run("Geometry", "Rectangle-toString", function(){
          var rect = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(0,0), new WAVE.Geometry.Point(100,100)); 
          log( rect );
          assertTrue("(0,0 ; 100x100)" === rect.toString());
       });



       run("Geometry", "vectorizeBalloon", function(){
          var body = new WAVE.Geometry.Rectangle( new WAVE.Geometry.Point(-100,-100), new WAVE.Geometry.Point(100,100)); 
          var target = new WAVE.Geometry.Point(0, 300);
          var legSweep = WAVE.Geometry.PI / 16;

          var points = WAVE.Geometry.vectorizeBalloon(body, target, legSweep);
          for(var i in points)
           log( points[i] );

          assertTrue( 7 === points.length );

          assertTrue( 100   === Math.floor( points[0].x() )); assertTrue( -100 === Math.floor( points[0].y() ));
          assertTrue( -100  === Math.floor( points[1].x() )); assertTrue( -100 === Math.floor( points[1].y() ));
          assertTrue( -100  === Math.floor( points[2].x() )); assertTrue(  100 === Math.floor( points[2].y() ));
          assertTrue( -10   === Math.floor( points[3].x() )); assertTrue(  100 === Math.floor( points[3].y() ));
          assertTrue( 0     === Math.floor( points[4].x() )); assertTrue(  300 === Math.floor( points[4].y() ));
          assertTrue( 9     === Math.floor( points[5].x() )); assertTrue(  100 === Math.floor( points[5].y() ));
          assertTrue(100 === Math.floor(points[6].x())); assertTrue(100 === Math.floor(points[6].y()));

          //   0,  -100
          //- 100, -100
          //- 100,    0
          //- 48,     0
          //0,      300
          //- 39,     0
          //0,        0
       });


               function TestObjectA(name){
                  WAVE.extend(this, WAVE.EventManager);
                  this.Name = name;
                  this.ShowCount = 0;
                  this.show = function(){ this.eventInvoke("before-show"); this.ShowCount++;  this.eventInvoke("after-show");};
                  this.hide = function(){ this.eventInvoke("before-hide"); this.ShowCount--;  this.eventInvoke("after-hide");};
               }
               TestObjectA.prototype.toString = function(){ return this.Name; };

               var eventTrace = "";
               function BeforeShow(sender) { eventTrace += (sender.Name+" before show Count= "+sender.ShowCount.toString()+", "); }
               function AfterShow (sender) { eventTrace += (sender.Name+" after show Count= "+sender.ShowCount.toString()+", "); }
               function BeforeHide(sender) { eventTrace += (sender.Name+" before hide Count= "+sender.ShowCount.toString()+", "); }
               function AfterHide (sender) { eventTrace += (sender.Name+" after hide Count= "+sender.ShowCount.toString()+", "); }


       run("Geometry", "rotateRectAroundCircle", function() {
          var rc = WAVE.Geometry.rotateRectAroundCircle(0, 0, 1, 4, 2, 0);
          assertTrue(4 === rc.width() && 2 === rc.height()); 
          assertTrue(devOk(1, rc.left()) && devOk(-1, rc.top())); 

          rc = WAVE.Geometry.rotateRectAroundCircle(0, 0, 1, 4, 2, Math.PI / 2);
          assertTrue(devOk(-2, rc.left()) && devOk(1, rc.top()));

          rc = WAVE.Geometry.rotateRectAroundCircle(0, 0, 1, 4, 2, Math.PI);
          assertTrue(devOk(-5, rc.left()) && devOk(-1, rc.top()));

          rc = WAVE.Geometry.rotateRectAroundCircle(0, 0, 1, 4, 2, Math.PI + Math.PI/2);
          assertTrue(devOk(-2, rc.left()) && devOk(-3, rc.top()));
       });


       run("EventManager", "bind", function(){
          var obj1 = new TestObjectA("Alex"); 
          var obj2 = new TestObjectA("Boris");
          
          eventTrace = "";

          obj1.eventBind("before-show", BeforeShow );
          obj1.eventBind("after-show",  AfterShow  );
          obj1.eventBind("before-hide", BeforeHide );
          obj1.eventBind("after-hide",  AfterHide  );

          obj2.eventBind("before-show", BeforeShow );
          obj2.eventBind("after-show",  AfterShow  );
          obj2.eventBind("before-hide", BeforeHide );
          obj2.eventBind("after-hide",  AfterHide  );
          
          
          obj1.show();
          obj1.show();
          obj1.hide();
          obj2.show();
          obj1.hide();
          obj2.hide();

          log( eventTrace );

          assertTrue(
           "Alex before show Count= 0, Alex after show Count= 1, Alex before show Count= 1, Alex after show Count= 2, Alex before hide Count= 2, "+
           "Alex after hide Count= 1, Boris before show Count= 0, Boris after show Count= 1, Alex before hide Count= 1, Alex after hide Count= 0, "+
           "Boris before hide Count= 1, Boris after hide Count= 0, " === eventTrace
          );
       });

       run("EventManager", "bind-suspended", function(){
          var obj1 = new TestObjectA("Alex"); 
          var obj2 = new TestObjectA("Boris");
          
          eventTrace = "";

          obj1.eventBind("before-show", BeforeShow );
          obj1.eventBind("after-show",  AfterShow  );
          obj1.eventBind("before-hide", BeforeHide );
          obj1.eventBind("after-hide",  AfterHide  );

          obj2.eventBind("before-show", BeforeShow );
          obj2.eventBind("after-show",  AfterShow  );
          obj2.eventBind("before-hide", BeforeHide );
          obj2.eventBind("after-hide",  AfterHide  );
          
          obj2.eventInvocationSuspendCount++;
          obj1.show();
          obj1.show();
          obj1.hide();
          obj2.show();
          obj1.hide();
          obj2.hide();

          obj2.eventInvocationSuspendCount--;

          log( eventTrace );

          assertTrue(
           "Alex before show Count= 0, Alex after show Count= 1, Alex before show Count= 1, Alex after show Count= 2, Alex before hide Count= 2, "+
           "Alex after hide Count= 1, Alex before hide Count= 1, Alex after hide Count= 0, " === eventTrace
          );
       });


       run("EventManager", "bind-unbind", function(){
          var obj1 = new TestObjectA("Alex"); 
          var obj2 = new TestObjectA("Boris");
          
          eventTrace = "";

          obj1.eventBind("before-show", BeforeShow );
          obj1.eventBind("after-show",  AfterShow  );
          obj1.eventBind("before-hide", BeforeHide );
          obj1.eventBind("after-hide",  AfterHide  );

          obj2.eventBind("before-show", BeforeShow );
          obj2.eventBind("after-show",  AfterShow  );
          obj2.eventBind("before-hide", BeforeHide );
          obj2.eventBind("after-hide",  AfterHide  );
          
          
          obj1.show();
          obj1.eventUnbind("before-show", BeforeShow);
          obj1.show();

          
          obj1.hide();
          obj2.show();
          obj1.hide();
          obj2.hide();

          log( eventTrace );

          assertTrue(
           "Alex before show Count= 0, Alex after show Count= 1, Alex after show Count= 2, Alex before hide Count= 2, "+
           "Alex after hide Count= 1, Boris before show Count= 0, Boris after show Count= 1, Alex before hide Count= 1, Alex after hide Count= 0, "+
           "Boris before hide Count= 1, Boris after hide Count= 0, " === eventTrace
          );
       });


       run("EventManager", "bind-clear-some", function(){
          var obj1 = new TestObjectA("Alex"); 
          var obj2 = new TestObjectA("Boris");
          
          eventTrace = "";

          obj1.eventBind("before-show", BeforeShow );
          obj1.eventBind("after-show",  AfterShow  );
          obj1.eventBind("before-hide", BeforeHide );
          obj1.eventBind("after-hide",  AfterHide  );

          obj2.eventBind("before-show", BeforeShow );
          obj2.eventBind("after-show",  AfterShow  );
          obj2.eventBind("before-hide", BeforeHide );
          obj2.eventBind("after-hide",  AfterHide  );
          
          
          obj1.show();
          obj1.eventClear("after-show");
          obj1.eventClear("after-hide");
          obj1.show();

          
          obj1.hide();
          obj2.show();
          obj1.hide();
          obj2.hide();

          log( eventTrace );

          assertTrue(
           "Alex before show Count= 0, Alex after show Count= 1, Alex before show Count= 1, Alex before hide Count= 2, "+
           "Boris before show Count= 0, Boris after show Count= 1, Alex before hide Count= 1, "+
           "Boris before hide Count= 1, Boris after hide Count= 0, " === eventTrace
          );
       });


       run("EventManager", "bind-clear-all", function(){
          var obj1 = new TestObjectA("Alex"); 
          var obj2 = new TestObjectA("Boris");
          
          eventTrace = "";

          obj1.eventBind("before-show", BeforeShow );
          obj1.eventBind("after-show",  AfterShow  );
          obj1.eventBind("before-hide", BeforeHide );
          obj1.eventBind("after-hide",  AfterHide  );

          obj2.eventBind("before-show", BeforeShow );
          obj2.eventBind("after-show",  AfterShow  );
          obj2.eventBind("before-hide", BeforeHide );
          obj2.eventBind("after-hide",  AfterHide  );
          
          
          obj1.show();
          obj1.eventClear();
          obj1.show();

          
          obj1.hide();
          obj2.show();
          obj1.hide();
          obj2.hide();

          log( eventTrace );

          assertTrue(
           "Alex before show Count= 0, Alex after show Count= 1, "+
           "Boris before show Count= 0, Boris after show Count= 1, "+
           "Boris before hide Count= 1, Boris after hide Count= 0, " === eventTrace
          );
       });


       run("EventManager", "bind-anyEvent", function(){
          var obj1 = new TestObjectA("Alex"); 
          var obj2 = new TestObjectA("Boris");
          
          eventTrace = "";
          var anyTrace = "";

          obj1.eventBind("before-show", BeforeShow );
          obj1.eventBind("after-show",  AfterShow  );
          obj1.eventBind("before-hide", BeforeHide );
          obj1.eventBind("after-hide",  AfterHide  );

          obj1.eventBind(WAVE.EventManager.ANY_EVENT, function(etype, sender)
                                                      {
                                                        anyTrace += etype + sender;
                                                      });

          obj2.eventBind("before-show", BeforeShow );
          obj2.eventBind("after-show",  AfterShow  );
          obj2.eventBind("before-hide", BeforeHide );
          obj2.eventBind("after-hide",  AfterHide  );
          obj2.eventBind(WAVE.EventManager.ANY_EVENT, function(etype, sender)
                                                      {
                                                        anyTrace += etype + sender;
                                                      });
          
          obj1.show();
          obj1.show();

          
          obj1.hide();
          obj2.show();
          obj1.hide();
          obj2.hide();

          log( eventTrace );
          log( anyTrace );

          assertTrue(
           "Alex before show Count= 0, Alex after show Count= 1, Alex before show Count= 1, Alex after show Count= 2, Alex before hide Count= 2, "+
           "Alex after hide Count= 1, Boris before show Count= 0, Boris after show Count= 1, Alex before hide Count= 1, Alex after hide Count= 0, "+
           "Boris before hide Count= 1, Boris after hide Count= 0, " === eventTrace
          );

          assertTrue("before-showAlexafter-showAlexbefore-showAlexafter-showAlexbefore-hideAlexafter-hideAlexbefore-showBorisafter-showBorisbefore-hideAlexafter-hideAlexbefore-hideBorisafter-hideBoris" === anyTrace);
       });


              function TestObjectB(name){
                  WAVE.extend(this, WAVE.EventManager);
                  this.Name = name;
                  this.Value = 0;
                  this.value1 = function(v){ this.Value = v; this.eventInvoke("value-change", v, true,false,-170);};
                  this.value2 = function(v){ this.Value = v; this.eventInvoke("value-change", {a: 1, b: "yes!"});};
                  this.value3 = function(v){ this.Value = v; this.eventInvoke("value-change");};
               }
               

    run("EventManager", "event-with-var-args-1", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          obj.eventBind("value-change", function(sender, v, flag1, flag2, intv){
            eventTrace += sender.Name+","+ v.toString()+","+flag1+","+flag2+","+intv;
          });

          obj.value1(123);

          log( eventTrace );

          assertTrue( "Alex,123,true,false,-170" === eventTrace  );
     });

     run("EventManager", "event-with-var-args-2", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          obj.eventBind("value-change", function(sender, obj){
            eventTrace += sender.Name+","+ obj.a.toString()+","+obj.b.toString();
          });

          obj.value2(123);

          log( eventTrace );

          assertTrue( "Alex,1,yes!" === eventTrace  );
     });

     run("EventManager", "event-with-var-args-3", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          obj.eventBind("value-change", function(sender, obj){
            eventTrace += sender.Name+","+ obj;
          });

          obj.value3(123);

          log( eventTrace );

          assertTrue( "Alex,undefined" === eventTrace  );
     });


     run("EventManager", "sinks-bind", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          obj.eventSinkBind( { a: 0,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             });
          obj.eventSinkBind( { a: 147,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             });
          


          obj.value3(123);

          log( eventTrace );
          assertTrue( "|0:value-changeAlex|147:value-changeAlex" === eventTrace  );
     });

     run("EventManager", "sinks-bind-unbind", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          var sink =  { a: 892,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             };


          obj.eventSinkBind( sink);
          


          obj.value3(123);

          log( eventTrace );
          assertTrue( "|892:value-changeAlex" === eventTrace  );

          obj.eventSinkUnbind(sink);
          eventTrace="NOTHING";
          obj.value3(567);

          log( eventTrace );
          assertTrue( "NOTHING" === eventTrace  );
     });


     run("EventManager", "sinks-clear", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          obj.eventSinkBind( { a: 0,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             });
          obj.eventSinkBind( { a: 147,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             });
          

          obj.eventSinkClear();

          obj.value3(123);

          log( eventTrace );
          assertTrue( "" === eventTrace  );
     });

     run("EventManager", "sinks-enumerate", function(){
          var obj = new TestObjectB("Alex"); 
          
          eventTrace = "";

          obj.eventSinkBind( { a: 11,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             });
          obj.eventSinkBind( { a: 147,              
                                   eventNotify: function(etype, sender)
                                            {
                                                eventTrace += "|"+this.a+":"+etype+sender.Name;
                                            }
                             });
          
          assertTrue( 2 === obj.eventSinks().length );
          assertTrue( 11 === obj.eventSinks()[0].a );
          assertTrue( 147 === obj.eventSinks()[1].a );
     });




     run("Walker", "walker-common", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);

          var walker = aw.getWalker(), i=0;
          while(walker.moveNext()) {
            assertTrue(a[i] === walker.current());
            i++;
          }

          assertFalse(walker.moveNext());
          assertTrue(null === walker.current());
     });

     run("Walker", "walker-reset", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);

          var walker = aw.getWalker();
          while(walker.moveNext());

          assertFalse(walker.moveNext());

          walker.reset();
          walker.moveNext();

          assertTrue(a[0] === walker.current());
     });

     run("Walker", "wWhere-1", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          aw = aw.wWhere(function(e) { return e > 3; });
          var whereWalkable = aw.wWhere(function(e) { return e > 3; });
          var resultArray = whereWalkable.wToArray();

          for(var i in resultArray) log(resultArray[i]);

          assertTrue( 2 === resultArray.length);
     });

     run("Walker", "wWhere-2", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wWhere1 = aw.wWhere(function(e) { return e > 3; });
          var wWhere2 = aw.wWhere(function(e) { return e > 2; });
          var result1 = wWhere1.wToArray();
          var result2 = wWhere2.wToArray();

          for(var i in result1) log(result1[i]);
          log("---");
          for(var j in result2) log(result2[j]);

          assertTrue( 2 === result1.length);
          assertTrue( 3 === result2.length);
     });

     run("Walker", "wWhere-chain", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wWhere1 = aw.wWhere(function(e) { return e > 1; });
          var wWhere2 = wWhere1.wWhere(function(e) { return e < 5; });
          var result2 = wWhere2.wToArray();

          for(var i in result2) log(result2[i]);

          assertTrue( 3 === result2.length);
     });

     run("Walker", "wTake1", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wTake = aw.wTake(3);
          var result = wTake.wToArray();

          for(var i in result) log(result[i]);

          assertTrue( 3 === result.length);
          assertTrue( a[0] === result[0]);
          assertTrue( a[1] === result[1]);
          assertTrue( a[2] === result[2]);
     });

     run("Walker", "wTake2", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wTake = aw.wTake(3);
          var walker = wTake.getWalker();

          for(var i=0; i<3; i++) {
            walker.moveNext();
            assertTrue(a[i] === walker.current());
          }

          assertFalse(walker.moveNext());
     });

     run("Walker", "wTakeWhile", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a).wTakeWhile(function(e) { return e < 3; });

          assertTrue(aw.wCount());
          assertTrue(1 === aw.wAt(0));
          assertTrue(2 === aw.wAt(1));
     });

     run("Walker", "wSkip-zero", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wSkip = aw.wSkip(0);
          var walker = wSkip.getWalker();

          var result = wSkip.wToArray();

          assertTrue(5 === result.length);
     });

     run("Walker", "wSkip-1", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wSkip = aw.wSkip(2);

          var result = wSkip.wToArray();

          assertTrue(3 === result.length);
          assertTrue(a[2] === result[0]);
          assertTrue(a[3] === result[1]);
          assertTrue(a[4] === result[2]);
     });

     run("Walker", "wSkip-exceed-length", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wSkip = aw.wSkip(5);

          var result = wSkip.wToArray();

          assertTrue(0 === result.length);
     });

     run("Walker", "wSelect", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var wSelect = aw.wSelect(function(e) { return e * 10; });
          var result = wSelect.wToArray();

          for(var i=0; i<a.length; i++) {
            assertTrue(a[i] * 10 === result[i]);
          }
     });

     run("Walker", "wSelectMany-wo-e2Walkable", function () {
           var a0 = [7, 1];
           var a1 = [2, -93, 15];

           var aa = [WAVE.arrayWalkable(a0), WAVE.arrayWalkable(a1)];
           var aw = WAVE.arrayWalkable(aa);
           var result = aw.wSelectMany();

           assertTrue(7 === result.wAt(0));
           assertTrue(1 === result.wAt(1));
           assertTrue(2 === result.wAt(2));
           assertTrue(-93 === result.wAt(3));
           assertTrue(15 === result.wAt(4));
     });

     run("Walker", "wSelectMany-e2Walkable-e", function () {
         var a0 = [7, 1];
         var a1 = [2, -93, 15];

         var aa = [WAVE.arrayWalkable(a0), WAVE.arrayWalkable(a1)];
         var aw = WAVE.arrayWalkable(aa);
         var result = aw.wSelectMany(function (e) { return e; });

         assertTrue(7 === result.wAt(0));
         assertTrue(1 === result.wAt(1));
         assertTrue(2 === result.wAt(2));
         assertTrue(-93 === result.wAt(3));
         assertTrue(15 === result.wAt(4));
     });

     run("Walker", "wSelectMany-e2Walkable-arr", function () {
         var a0 = [7, 1];
         var a1 = [2, -93, 15];

         var aa = [a0, a1];
         var aw = WAVE.arrayWalkable(aa);
         var result = aw.wSelectMany(function (e) { return WAVE.arrayWalkable(e); });

         assertTrue(7 === result.wAt(0));
         assertTrue(1 === result.wAt(1));
         assertTrue(2 === result.wAt(2));
         assertTrue(-93 === result.wAt(3));
         assertTrue(15 === result.wAt(4));
     });

     run("Walker", "wSelectMany-e2Walkable-arr-empty", function () {
       var a0 = [];
       var a1 = null;
       var a2 = [2, -93, 15];

       var aa = [a0, a1, a2];
       var aw = WAVE.arrayWalkable(aa);
       var result = aw.wSelectMany(function (e) { return WAVE.arrayWalkable(e); });

       assertTrue(2 === result.wAt(0));
       assertTrue(-93 === result.wAt(1));
       assertTrue(15 === result.wAt(2));
     });

     run("Walker", "wSelectMany-e2Walkable-reset", function () {
       var a0 = [7, 1];
       var a1 = [2, -93, 15];

       var aa = [a0, a1];
       var aw = WAVE.arrayWalkable(aa);
       var result = aw.wSelectMany(function (e) { return WAVE.arrayWalkable(e); });

       var walker = result.getWalker();

       walker.moveNext();
       assertTrue(7 === walker.current());

       walker.reset();
       walker.moveNext();
       assertTrue(7 === walker.current());
     });

     run("Walker", "wAt", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var at;

          at = aw.wAt(0);
          assertTrue(a[0] === at);

          at = aw.wAt(4);
          assertTrue(a[4] === at);

          at = aw.wAt(-1);
          assertTrue(null === at);

          at = aw.wAt(5);
          assertTrue(null === at);
     });

     run("Walker", "wFirst", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var first;

          first = aw.wFirst();
          assertTrue(a[0] === first);

          first = aw.wFirst(function(e) { return e > 2; });
          assertTrue(a[2] === first);

          first = aw.wFirst(function(e) { return e < 0; });
          assertTrue(null === first);

          aw = WAVE.arrayWalkable([]);
          cnt = aw.wFirst();
          assertTrue(null === cnt);
     });

     run("Walker", "wFirstIdx", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var idx;

          idx = aw.wFirstIdx(function(e) { return e === 0; });
          assertTrue(-1 === idx);

          idx = aw.wFirstIdx(function(e) { return e === 1; });
          assertTrue(0 === idx);

          idx = aw.wFirstIdx(function(e) { return e === 5; });
          assertTrue(4 === idx);
     });

     run("Walker", "wCount", function() {
          var a = [1, 2, 3, 4, 5]; 

          var aw = WAVE.arrayWalkable(a);
          var cnt;

          cnt = aw.wCount();
          assertTrue(5 === cnt);

          cnt = aw.wCount(function(e) { return e > 2; });
          assertTrue(3 === cnt);

          aw = WAVE.arrayWalkable([]);
          cnt = aw.wCount();
          assertTrue(0 === cnt);
     });

     run("Walker", "wDistinct-wo-equal", function() {
          var a = [1, 2, 2, 3, 5, 4, 2, 4, 3]; 

          var aw = WAVE.arrayWalkable(a);
          var distinct = aw.wDistinct();
          var result = distinct.wToArray();

          assertTrue(5 === result.length);
          assertTrue(1 === result[0]);
          assertTrue(2 === result[1]);
          assertTrue(3 === result[2]);
          assertTrue(5 === result[3]);
          assertTrue(4 === result[4]);
     });

     run("Walker", "wDistinct-with-equal", function() {
          var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 2, y: 1}, {x: 3, y: 1}, {x: 4, y: 3}];

          var aw = WAVE.arrayWalkable(a);
          var distinct = aw.wDistinct(function(a, b) { return a.y === b.y; });
          var result = distinct.wToArray();

          assertTrue(3 === result.length);
          assertTrue(0 === result[0].x && 0 === result[0].y);
          assertTrue(1 === result[1].x && 1 === result[1].y);
          assertTrue(4 === result[2].x && 3 === result[2].y);
     });

     run("Walker", "wConcat-with-empty", function() {
          var aw = WAVE.arrayWalkable([0, 1, 2]);
          var bw = WAVE.arrayWalkable([]);
          var concat = aw.wConcat(bw);

          assertTrue(3 === concat.wCount());
          assertTrue(0 === concat.wAt(0));
          assertTrue(1 === concat.wAt(1));
          assertTrue(2 === concat.wAt(2));

          concat = bw.wConcat(aw);

          assertTrue(3 === concat.wCount());
          assertTrue(0 === concat.wAt(0));
          assertTrue(1 === concat.wAt(1));
          assertTrue(2 === concat.wAt(2));
     });

     run("Walker", "wConcat-regular", function() {
          var aw = WAVE.arrayWalkable([4, 7, 12]);
          var bw = WAVE.arrayWalkable([6,-39]);
          var concat = aw.wConcat(bw);

          assertTrue(5 === concat.wCount());
          assertTrue(4 === concat.wAt(0));
          assertTrue(7 === concat.wAt(1));
          assertTrue(12 === concat.wAt(2));
          assertTrue(6 === concat.wAt(3));
          assertTrue(-39 === concat.wAt(4));
     });

     run("Walker", "wExcept-with-empty", function() {
          var a = [1, 2, 3];
          var b = [];

          var aw = WAVE.arrayWalkable(a);
          var bw = WAVE.arrayWalkable(b);
          
          var except = aw.wExcept(bw);

          assertTrue(3 === except.wCount());
          assertTrue(1 === except.wAt(0));
          assertTrue(2 === except.wAt(1));
          assertTrue(3 === except.wAt(2));
     });

     run("Walker", "wExcept-with-other", function() {
          var a = [1, 2, 3];
          var b = [2];

          var aw = WAVE.arrayWalkable(a);
          var bw = WAVE.arrayWalkable(b);
          
          var except = aw.wExcept(bw);

          assertTrue(2 === except.wCount());
          assertTrue(1 === except.wAt(0));
          assertTrue(3 === except.wAt(1));

          a = [1, 2, 3];
          b = [1];

          aw = WAVE.arrayWalkable(a);
          bw = WAVE.arrayWalkable(b);
          
          except = aw.wExcept(bw);

          assertTrue(2 === except.wCount());
          assertTrue(2 === except.wAt(0));
          assertTrue(3 === except.wAt(1));
     });

     run("Walker", "wExcept-with-equal", function() {
          var a = [{k: 3, v: "a"}, {k: 1, v: "b"}, {k: 2, v: "c"}];
          var b = [{k: 1, v: "b"}, {k: 2, v: "a"}];

          var aw = WAVE.arrayWalkable(a);
          var bw = WAVE.arrayWalkable(b);
          
          var except = aw.wExcept(bw, function(e0, e1) { return e0.v === e1.v;});

          assertTrue(1 === except.wCount());
          assertTrue(2 === except.wAt(0).k); assertTrue("c" === except.wAt(0).v);
     });

     run("Walker", "wExcept-diff-type-seq", function() {
          var a = [1, 2, 3];
          var b = [{id: 2}];

          var aw = WAVE.arrayWalkable(a);
          var bw = WAVE.arrayWalkable(b);
          
          var except = aw.wExcept(bw, function(el0, el1) { return el0 === el1.id; });

          assertTrue(2 === except.wCount());
          assertTrue(1 === except.wAt(0));
          assertTrue(3 === except.wAt(1));
     });

     run("Walker", "wGroup", function() {
          var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

          var group = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; });
          assertTrue(34 === group.wAt(0).v.wAt(2));
     });

     run("Walker", "wGroupIntoObject", function() {
          var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

          var group = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; }).wGroupIntoObject();

          assertTrue(4 === group['0'].length);
          assertTrue(2 === group['1'].length);

          assertTrue(34 === group['0'][2]);
          assertTrue(127 === group['0'][3]);

          assertTrue(1 === group['1'][0]);
          assertTrue(-5 === group['1'][1]);
     });

     run("Walker", "wGroupIntoArray", function() {
          var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

          var group = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; }).wGroupIntoArray();

          assertTrue(0 === group[0].k);
          assertTrue(4 === group[0].v.length);
              assertTrue(0 === group[0].v[0]);
              assertTrue(33 === group[0].v[1]);
              assertTrue(34 === group[0].v[2]);
              assertTrue(127 === group[0].v[3]);

          assertTrue(1 === group[1].k);
          assertTrue(2 === group[1].v.length);
              assertTrue(1 === group[1].v[0]);
              assertTrue(-5 === group[1].v[1]);
     });

     run("Walker", "groupWalkable", function() {
          var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

          var groupArr = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; }).wGroupIntoArray();

          var groupWalkable = WAVE.groupWalkable(groupArr);

          assertTrue(0 === groupWalkable.wAt(0).k);
            assertTrue(0 === groupWalkable.wAt(0).v.wAt(0));
            assertTrue(33 === groupWalkable.wAt(0).v.wAt(1));
            assertTrue(34 === groupWalkable.wAt(0).v.wAt(2));
            assertTrue(127 === groupWalkable.wAt(0).v.wAt(3));

          assertTrue(1 === groupWalkable.wAt(1).k);
            assertTrue(1 === groupWalkable.wAt(1).v.wAt(0));
            assertTrue(-5 === groupWalkable.wAt(1).v.wAt(1));
     });
     
     run("Walker", "wEach", function() {
          var a = [{x: 0, y: 0}, {x: 1, y: 18}];

          WAVE.arrayWalkable(a).wEach(function(e, i) { log(e.x + e.y); log(i); });
     });

     run("Walker", "wOrder-wo-functor", function() {
          var a = [1, -3, 2, 17, -90, 4, 55];

          var ordered = WAVE.arrayWalkable(a).wOrder().wToArray();

          assertTrue(-90 === ordered[0]);
          assertTrue(-3 === ordered[1]);
          assertTrue(1 === ordered[2]);
          assertTrue(2 === ordered[3]);
          assertTrue(4 === ordered[4]);
          assertTrue(17 === ordered[5]);
          assertTrue(55 === ordered[6]);
     });

     run("Walker", "wOrder-with-functor", function() {
          var a = [1, -3, 2, 17, -90, 4, 55];

          var ordered = WAVE.arrayWalkable(a).wOrder(function(a, b) { return a < b ? 1 : a > b ? -1 : 0; }).wToArray();

          assertTrue(-90 === ordered[6]);
          assertTrue(-3 === ordered[5]);
          assertTrue(1 === ordered[4]);
          assertTrue(2 === ordered[3]);
          assertTrue(4 === ordered[2]);
          assertTrue(17 === ordered[1]);
          assertTrue(55 === ordered[0]);
     });

     run("Walker", "wAny", function() {
          var a = [1, -3, 2, 17, -90, 4, 55];

          var any = WAVE.arrayWalkable(a).wAny();
          assertTrue(any);

          any = WAVE.arrayWalkable(a).wAny(function(e) { return e === -90; });
          assertTrue(any);
          
          any = WAVE.arrayWalkable(a).wAny(function(e) { return e === 150; });
          assertFalse(any);
     });

     run("Walker", "wAll", function() {
          var a = [1, -3, 2, 17, -90, 4, 55];

          var all = WAVE.arrayWalkable([]).wAll();
          assertTrue(all);

          all = WAVE.arrayWalkable([1,1,1]).wAll(function(e) { return e === 1; });
          assertTrue(all);
          
          all = WAVE.arrayWalkable([1,1,2]).wAll(function(e) { return e === 1; });
          assertFalse(all);
     });

     run("Walker", "wMin", function() {
          var min;

          min = WAVE.arrayWalkable([]).wMin();
          assertTrue(null === min);

          min = WAVE.arrayWalkable([1, -3, 2, 17, -90, 4, 55]).wMin();
          assertTrue(-90 === min);

          min = WAVE.arrayWalkable([1, -3, 2, 17, -90, 4, 55]).wMin(function(a, b) { return a > b;});
          assertTrue(55 === min);
     });

     run("Walker", "wMax", function() {
          var max;

          max = WAVE.arrayWalkable([]).wMax();
          assertTrue(null === max);

          max = WAVE.arrayWalkable([1, -3, 2, 17, -90, 4, 55]).wMax();
          assertTrue(55 === max);

          max = WAVE.arrayWalkable([1, -3, 2, 17, -90, 4, 55]).wMax(function(a, b) { return a < b;});
          assertTrue(-90 === max);
     });

     run("Walker", "wAggregate", function() {
          var agr;

          agr = WAVE.arrayWalkable([]).wAggregate(function(r, e) { r = (r || 0) + e;});
          assertTrue("undefined" === typeof(agr));

          agr = WAVE.arrayWalkable([]).wAggregate(function(r, e) { r = (r || 0) + e;}, 0);
          assertTrue(0 === agr);

          agr = WAVE.arrayWalkable([0]).wAggregate(function(r, e) { return (r || 0) + e;});
          assertTrue(0 === agr);

          agr = WAVE.arrayWalkable([0, 1, 4, 25, -3]).wAggregate(function(r, e) { return (r || 0) + e;});
          assertTrue(27 === agr);

          agr = WAVE.arrayWalkable([0, 1, 4, 25, -3]).wAggregate(function(r, e) { return (r || 0) - e;});
          assertTrue(-27 === agr);
     });

     run("Walker", "wGroupAggregate", function() {
          var gAgr;

          var a = [{x: 0, y: 0}, {x: 1, y: 1}, {x: 0, y: 33}, {x: 0, y: 34}, {x: 1, y: -5}, {x: 0, y: 127}];

          var group = WAVE.arrayWalkable(a).wGroup(function(e) { return e.x; }, function(e) { return e.y; }).wGroupAggregate(function(k, r, e) { return (r || 0) + e; });

          assertTrue(194 === group.wAt(0).v);
          assertTrue(-4 === group.wAt(1).v);
     });

     run("Walker", "wSum", function() {
          var sum;

          sum = WAVE.arrayWalkable([]).wSum();
          assertTrue(0 === sum);

          sum = WAVE.arrayWalkable([]).wSum(0);
          assertTrue(0 === sum);

          sum = WAVE.arrayWalkable([]).wSum(1);
          assertTrue(1 === sum);

          sum = WAVE.arrayWalkable([0, 1, 4, 25, -3]).wSum();
          assertTrue(27 === sum);
     });

     run("Walker", "wEqual-default-comparison", function() {
          assertTrue(WAVE.arrayWalkable([]).wEqual(WAVE.arrayWalkable([])));
          assertTrue(WAVE.arrayWalkable([0]).wEqual(WAVE.arrayWalkable([0])));
          assertTrue(WAVE.arrayWalkable([-1, 1, 34.53]).wEqual(WAVE.arrayWalkable([-1, 1, 34.53])));
          assertFalse(WAVE.arrayWalkable([0, 1]).wEqual(WAVE.arrayWalkable([0])));
          assertFalse(WAVE.arrayWalkable([0, 1]).wEqual(WAVE.arrayWalkable([0, 1, 2])));
          assertFalse(WAVE.arrayWalkable([0, 1]).wEqual(WAVE.arrayWalkable([0, 2])));
     });

     run("Walker", "wEqual-equalFunc-comparison", function() {
          assertTrue(WAVE.arrayWalkable([0, 1, 2]).wEqual(WAVE.arrayWalkable([0, 1, 2]), function(a, b) { return a === b;}));
          assertFalse(WAVE.arrayWalkable([0, 1, 2]).wEqual(WAVE.arrayWalkable([0, 1, 2]), function(a, b) { return a !== b;}));
          assertTrue(WAVE.arrayWalkable([0, 1, 4]).wEqual(WAVE.arrayWalkable([0, 3, 6]), function(a, b) { return a % 2 === b % 2;}));
     });

     run("Walker", "composite", function() {
          var data = [
             {ID: "K1234", FirstName: "James", LastName: "Bander", DOB: "12/10/1982", Sex: "M"},
             {ID: "O8982", FirstName: "Ann", LastName: "Dumper", DOB: "2/14/1983", Sex: "F"},
             {ID: "A7807", FirstName: "Josh", LastName: "Alexanderesku", DOB: "2/12/1965", Sex: "M"},
             {ID: "L0201", FirstName: "Anna", LastName: "Mendoza", DOB: "07/02/1979", Sex: "F"},
             {ID: "A4503", FirstName: "Chuck", LastName: "Bonderville", DOB: "11/2/1992", Sex: "M"},
             {ID: "Z9029", FirstName: "Eugene", LastName: "Kutz", DOB: "09/28/1930", Sex: "M"},
             {ID: "P8923", FirstName: "Ann", LastName: "Nemo", DOB: "05/19/1978", Sex: "F"}
          ];

          var all = WAVE.arrayWalkable(data);
       
          assertTrue( 4 === all.wCount(  function(e){ return e.Sex==="M"; } ) );
          assertTrue( 3 === all.wCount(  function(e){ return e.Sex==="F"; } ) );
          assertTrue( 2 === all.wDistinct( function(a,b){return a.FirstName===b.FirstName;} ).wCount(  function(e){ return e.Sex==="F"; } ) );


          all       
           .wSelect( function(e) { return { Name: e.FirstName, Sex: e.Sex, EOB: WAVE.convertScalarType(false, e.DOB, "date").getFullYear()}; } )
           .wWhere( function(e){ return e.EOB < 1990; } )
           .wGroup( function(e) { return e.Sex; } , function(e) { return {Name: e.Name, EOB: e.EOB}; })
           .wEach( function(grp)
                  {
                    log("Gender Group: "+grp.k);
                    log("---------------------");
                    grp.v.wOrder( function(a,b){ return a.Name<b.Name ? -1 : a.Name>b.Name ? +1 : 0; })
                          .wEach( function(e){ log(e.Name+"   "+e.EOB); });
                  });
           
     });

     run("Walker", "wWMA-no-momentum", function() {
          var aw = WAVE.arrayWalkable([{s: 0, a: 1}, {s: 1, a: 10}, {s: 2, a: 20}, {s: 3, a: 0}, {s: 4, a: -5}, {s: 5, a: -10}]);
          var wma = aw.wWMA(0);

          assertTrue( wma.wEqual(aw, function(a, b) { return a.s === b.s && a.a === b.a;}));
     });

     run("Walker", "wWMA-infinite-momentum", function() {
          var aw = WAVE.arrayWalkable([{s: 0, a: 1}, {s: 1, a: 10}, {s: 2, a: 20}, {s: 3, a: 0}, {s: 4, a: -5}, {s: 5, a: -10}]);
          var wma = aw.wWMA(1);

          wma.wEach(function(e) { assertTrue(1 === e.a); });
     });

     run("Walker", "wWMA-default-momentum", function() {
          var aw = WAVE.arrayWalkable([{s: 0, a: 1}, {s: 1, a: 10}, {s: 2, a: 20}]);
          var wma = aw.wWMA();

          assertTrue(1 === wma.wAt(0).a);
          assertTrue(5.5 === wma.wAt(1).a);
          assertTrue(12.75 === wma.wAt(2).a);
     });

     run("Walker", "wWMA-custom-momentum", function() {
          var aw = WAVE.arrayWalkable([{s: 0, a: 1}, {s: 1, a: 100}, {s: 2, a: -100}]);
          var wma = aw.wWMA(0.1);

          assertTrue(1 === wma.wAt(0).a);
          assertTrue(90.1 === wma.wAt(1).a);
          assertTrue(-80.99 === wma.wAt(2).a);
     });

     run("Walker", "Signal-signalConstSrc", function() {
          var cs = WAVE.signalConstSrc();
          var walker = cs.getWalker();
          var c;

          walker.moveNext(); c = walker.current();
          assertTrue(0 === c.s && 0 === c.a);

          walker.moveNext(); c = walker.current();
          assertTrue(1 === c.s && 0 === c.a);
     });

     run("Walker", "Signal-signalConstSrc-qty-amplitude", function() {
          var cs = WAVE.signalConstSrc({a: 17.75, qty: 2});

          assertTrue(2 === cs.wCount());

          var walker = cs.getWalker();

          walker.moveNext(); 
          assertTrue(17.75 === walker.current().a);

          walker.moveNext(); 
          assertTrue(17.75 === walker.current().a);

          assertFalse(walker.moveNext());
     });

     run("Walker", "wSineGen", function() {
          var sine = WAVE.signalConstSrc({qty: 10}).wSineGen({r: 4});

          assertTrue(devOk(0, sine.wAt(0).a)); assertTrue(0 === sine.wAt(0).s);
          assertTrue(devOk(1, sine.wAt(1).a)); assertTrue(1 === sine.wAt(1).s);
          assertTrue(devOk(0, sine.wAt(2).a)); assertTrue(2 === sine.wAt(2).s);
          assertTrue(devOk(-1, sine.wAt(3).a)); assertTrue(3 === sine.wAt(3).s);
          assertTrue(devOk(0, sine.wAt(4).a)); assertTrue(4 === sine.wAt(4).s);
     });

     run("Walker", "wSineGen-samplingRate-1000", function() {
          var sine = WAVE.signalConstSrc().wSineGen({r: 1000});

          assertTrue(devOk(0, sine.wAt(0).a));
          assertTrue(devOk(1, sine.wAt(250).a));
          assertTrue(devOk(0, sine.wAt(500).a));
          assertTrue(devOk(-1, sine.wAt(750).a));
          assertTrue(devOk(0, sine.wAt(1000).a));
     });

     run("Walker", "wSineGen-amplitude-2", function() {
          var sine = WAVE.signalConstSrc().wSineGen({a: 2, r: 4});

          assertTrue(devOk(0, sine.wAt(0).a));
          assertTrue(devOk(2, sine.wAt(1).a));
          assertTrue(devOk(0, sine.wAt(2).a));
          assertTrue(devOk(-2, sine.wAt(3).a));
          assertTrue(devOk(0, sine.wAt(4).a));
     });

     run("Walker", "wRandomGen", function() {
          var rnd = WAVE.signalConstSrc({qty: 7}).wRandomGen();

          rnd.wEach(function(e) { log(e.s + "->" + e.a); });

          rnd.wEach(function(e) { assertTrue(Math.abs(e.a) < 1);});
     });

     run("Walker", "wRandomGen-amplitude-2", function() {
          var rnd = WAVE.signalConstSrc({qty: 20}).wRandomGen().wSelect(function(e) { return Math.abs(e.a); }).wSum();
          var rnd2 = WAVE.signalConstSrc({qty: 20}).wRandomGen({a: 2}).wSelect(function(e) { return Math.abs(e.a); }).wSum();

          log(rnd + ", " + rnd2);

          assertTrue(rnd2 >= rnd * 1.4);

          if (rnd2 < rnd * 1.5) log("Test can fail sometimes according to the fact that Math.random() is used. Please re-run.");
     });

     run("Walker", "wSineGen-frequency-2", function() {
          var sine = WAVE.signalConstSrc().wSineGen({f: 2, r: 8});

          assertTrue(devOk(0, sine.wAt(0).a));
          assertTrue(devOk(1, sine.wAt(1).a));
          assertTrue(devOk(0, sine.wAt(2).a));
          assertTrue(devOk(-1, sine.wAt(3).a));
          assertTrue(devOk(0, sine.wAt(4).a));
     });

     run("Walker", "wSineGen-d-minus1", function() {
          var sine = WAVE.signalConstSrc().wSineGen({d: -1, r: 4});

          assertTrue(devOk(-1, sine.wAt(0).a));
          assertTrue(devOk(0, sine.wAt(1).a));
          assertTrue(devOk(-1, sine.wAt(2).a));
          assertTrue(devOk(-2, sine.wAt(3).a));
          assertTrue(devOk(-1, sine.wAt(4).a));
     });

     run("Walker", "wSineGen-phase-half-pi", function() {
          var sine = WAVE.signalConstSrc().wSineGen({p: -Math.PI/2, r: 4});

          assertTrue(devOk(-1, sine.wAt(0).a));
          assertTrue(devOk(0, sine.wAt(1).a));
          assertTrue(devOk(1, sine.wAt(2).a));
          assertTrue(devOk(0, sine.wAt(3).a));
          assertTrue(devOk(-1, sine.wAt(4).a));
     });

     run("Walker", "wSawGen", function() {
          var saw = WAVE.signalConstSrc().wSawGen({r: 20});

          assertTrue(devOk(-1, saw.wAt(0).a)); assertTrue(0 === saw.wAt(0).s);
          assertTrue(devOk(0, saw.wAt(9).a)); assertTrue(1 === saw.wAt(1).s);
          assertTrue(devOk(1, saw.wAt(18).a)); assertTrue(2 === saw.wAt(2).s);
          assertTrue(devOk(0, saw.wAt(19).a)); assertTrue(3 === saw.wAt(3).s);
          assertTrue(devOk(-1, saw.wAt(20).a)); assertTrue(4 === saw.wAt(4).s);
     });
     
     run("Walker", "wSawGen-symmetry-.5", function() {
          var saw = WAVE.signalConstSrc().wSawGen({s: 0.5, r: 4});

          assertTrue(devOk(-1, saw.wAt(0).a));
          assertTrue(devOk(0, saw.wAt(1).a));
          assertTrue(devOk(1, saw.wAt(2).a));
          assertTrue(devOk(0, saw.wAt(3).a));
          assertTrue(devOk(-1, saw.wAt(4).a));
     });

     run("Walker", "wSquareGen", function() {
          var square = WAVE.signalConstSrc().wSquareGen({r: 40});

          assertTrue(devOk(1, square.wAt(0).a));
          assertTrue(devOk(1, square.wAt(18).a));
          assertTrue(devOk(-1, square.wAt(20).a));
          assertTrue(devOk(-1, square.wAt(38).a));
          assertTrue(devOk(1, square.wAt(40).a));
     });

     run("Walker", "wSquareGen-symmetry-.5", function() {
          var square = WAVE.signalConstSrc().wSquareGen({s: 0.5, r: 40});

          assertTrue(devOk(1, square.wAt(0).a));
          assertTrue(devOk(1, square.wAt(10).a));
          assertTrue(devOk(-1, square.wAt(20).a));
          assertTrue(devOk(-1, square.wAt(30).a));
          assertTrue(devOk(1, square.wAt(40).a));
     });

     run("Walker", "wConstLinearSampling-empty-input", function() {
          assertTrue(0 === WAVE.arrayWalkable([]).wConstLinearSampling(2).wCount());
     });

     run("Walker", "wConstLinearSampling-reset", function() {
          var walker = WAVE.arrayWalkable([{s: 1, a: 10}, {s: 2, a: 20}, {s: 3, a: 30}]).wConstLinearSampling().getWalker();

          walker.moveNext();
          assertTrue(0 === walker.current().s);
          assertTrue(10 === walker.current().a);

          walker.moveNext();
          assertTrue(1 === walker.current().s);
          assertTrue(20 === walker.current().a);

          walker.reset();
          walker.moveNext();
          assertTrue(0 === walker.current().s);
          assertTrue(10 === walker.current().a);
     });

     run("Walker", "wConstLinearSampling-first-sample", function() {
          var w = WAVE.arrayWalkable([{s: 1, a: 10}, {s: 10, a: 700}]).wConstLinearSampling(2);

          var el0 = w.wAt(0);

          assertTrue(0 === el0.s);
          assertTrue(10 === el0.a);
     });

     run("Walker", "wConstLinearSampling-024->01234", function() {
          var w = WAVE.arrayWalkable([{s: 0, a: 10}, {s: 2, a: 10}, {s: 4, a: 10}]).wConstLinearSampling(1);

          assertTrue(5 === w.wCount());
     });

     run("Walker", "wConstLinearSampling-01234->024", function() {
          var w = WAVE.arrayWalkable([{s: 0, a: 10}, {s: 1, a: 10}, {s: 2, a: 10}, {s: 3, a: 10}, {s: 4, a: 10}]).wConstLinearSampling(2);

          assertTrue(3 === w.wCount());
     });

     run("Walker", "wConstLinearSampling-0_4->0_2_4", function() {
          var w = WAVE.arrayWalkable([{s: 0, a: 0}, {s: 4, a: 10}]).wConstLinearSampling(2);

          assertTrue(3 === w.wCount());

          assertTrue(0 === w.wAt(0).s && 0 === w.wAt(0).a);
          assertTrue(1 === w.wAt(1).s && 5 === w.wAt(1).a);
          assertTrue(2 === w.wAt(2).s && 10 === w.wAt(2).a);
     });

     run("Walker", "wConstLinearSampling-0_1_2_3->0_2_4", function() {
          var w = WAVE.arrayWalkable([{s: 0, a: 10}, {s: 1, a: 20}, {s: 2, a: 30}, {s: 3, a: 40}, {s: 4, a: 50}]).wConstLinearSampling(2);

          assertTrue(3 === w.wCount());

          assertTrue(0 === w.wAt(0).s && 15 === w.wAt(0).a);
          assertTrue(1 === w.wAt(1).s && 35 === w.wAt(1).a);
          assertTrue(2 === w.wAt(2).s && 50 === w.wAt(2).a);
     });

     run("Walker", "wConstLinearSampling-step-.5", function() {
          var w = WAVE.arrayWalkable([{s: 0, a: 10}, {s: 1, a: 20}, {s: 2, a: 10}]).wConstLinearSampling(0.5);

          assertTrue(5 === w.wCount());

          assertTrue(0 === w.wAt(0).s && 10 === w.wAt(0).a);
          assertTrue(1 === w.wAt(1).s && 15 === w.wAt(1).a);
          assertTrue(2 === w.wAt(2).s && 20 === w.wAt(2).a);
          assertTrue(3 === w.wAt(3).s && 15 === w.wAt(3).a);
          assertTrue(4 === w.wAt(4).s && 10 === w.wAt(4).a);
     });

     run("Walker", "wConstLinearSampling-both-compression-decompression", function() {
          var w = WAVE.arrayWalkable([{s: 0, a: 10}, {s: 1, a: 20}, {s: 2, a: 10}, {s: 10, a: 100}, {s: 20, a: 500}]).wConstLinearSampling(2);

          

          assertTrue(11 === w.wCount());

          assertTrue(0 === w.wAt(0).s && 15 === w.wAt(0).a);
          assertTrue(1 === w.wAt(1).s && 10 === w.wAt(1).a);
          assertTrue(2 === w.wAt(2).s && 32.5 === w.wAt(2).a);
          assertTrue(3 === w.wAt(3).s && 55 === w.wAt(3).a);
          assertTrue(4 === w.wAt(4).s && 77.5 === w.wAt(4).a);

          assertTrue(5 === w.wAt(5).s && 100 === w.wAt(5).a);
          assertTrue(6 === w.wAt(6).s && 180 === w.wAt(6).a);
          assertTrue(7 === w.wAt(7).s && 260 === w.wAt(7).a);
          assertTrue(8 === w.wAt(8).s && 340 === w.wAt(8).a);
          assertTrue(9 === w.wAt(9).s && 420 === w.wAt(9).a);
          assertTrue(10 === w.wAt(10).s && 500 === w.wAt(10).a);
     });

(function ($W) {
  run("Markup", "markup", function () {
    checkMarkup('<',      '<p>&lt;</p>');
    checkMarkup('>',      '<p>&gt;</p>');
    checkMarkup('&copy;', '<p>&copy;</p>');
    checkMarkup('& ;',    '<p>&nbsp;</p>');
    checkMarkup('&!;',    '<p>&#33;</p>');
    checkMarkup('&#;',    '<p>&#35;</p>');
    checkMarkup('&$;',    '<p>&#36;</p>');
    checkMarkup('&*;',    '<p>&#42;</p>');
    checkMarkup('&<;',    '<p>&lt;</p>');
    checkMarkup('&=;',    '<p>&#61;</p>');
    checkMarkup('&>;',    '<p>&gt;</p>');
    checkMarkup('&{;',    '<p>&#123;</p>');
    checkMarkup('&};',    '<p>&#125;</p>');
    checkMarkup('<script></script>', '<p>&lt;script&gt;&lt;/script&gt;</p>');
  });
  run("Markup", "markup-p", function () {
    checkMarkup('',         '');
    checkMarkup('a',        '<p>a</p>');
    checkMarkup('\na',      '<p>a</p>');
    checkMarkup('\n\na',    '<p>a</p>');
    checkMarkup('\n\n\na',  '<p>a</p>');
    checkMarkup('a\n',      '<p>a</p>');
    checkMarkup('a\n\n',    '<p>a</p>');
    checkMarkup('a\n\n\n',  '<p>a</p>');
    checkMarkup('a\nb',     '<p>a b</p>');
    checkMarkup('a\n\nb',   '<p>a</p><p>b</p>');
    checkMarkup('a\n\n\nb', '<p>a</p><p>b</p>');
  });
  run("Markup", "markup-span", function () {
    checkMarkup('{}',       '<p></p>');
    checkMarkup('{}\n',     '<p></p>');
    checkMarkup('{}\na',    '<p> a</p>');
    checkMarkup('{}.',      '<p>.</p>');
    checkMarkup('{}.\n',    '<p>.</p>');
    checkMarkup('{}.\na',   '<p>. a</p>');
    checkMarkup('{}..',     '<p>..</p>');
    checkMarkup('{}..a..',  '<p>..a..</p>');
    checkMarkup('..{}..',   '<p>....</p>');
    checkMarkup('{a}',      '<p>a</p>');
    checkMarkup('{{}',      '<p>{</p>');
    checkMarkup('{}.a',     '<p><span class="wv-markup-a"></span></p>');
    checkMarkup('{{}.a}.b', '<p><span class="wv-markup-b"><span class="wv-markup-a"></span></span></p>');
    checkMarkup('{\n}.a',   '<p><span class="wv-markup-a"> </span></p>');
    checkMarkup('{\n\n}.a', '<p><span class="wv-markup-a">  </span></p>');
    checkMarkup('{}.a.',    '<p><span class="wv-markup-a"></span>.</p>');
    checkMarkup('{}.a..',   '<p><span class="wv-markup-a"></span>..</p>');
    checkMarkup('{}.a.a',   '<p><span class="wv-markup-a"></span></p>');
    checkMarkup('{}.a.b',   '<p><span class="wv-markup-a wv-markup-b"></span></p>');
    checkMarkup('{}.ab',    '<p><span class="wv-markup-ab"></span></p>');
    checkMarkup('{}.ab.ac', '<p><span class="wv-markup-ab wv-markup-ac"></span></p>');
    checkMarkup('{}.a..b',  '<p><span class="wv-markup-a"></span>..b</p>');
    checkMarkup('{{{}.a',   '<p>{{<span class="wv-markup-a"></span></p>');
  });
  run("Markup", "markup-heading", function () {
    checkMarkup('!',           '<p>!</p>');
    checkMarkup('!\n',         '<p>!</p>');
    checkMarkup('!\na',        '<p>! a</p>');
    checkMarkup(' !',          '<p> !</p>');
    checkMarkup('! ',          '<h1> </h1>');
    checkMarkup('!a',          '<h1>a</h1>');
    checkMarkup('!!a',         '<h2>a</h2>');
    checkMarkup('!!!a',        '<h3>a</h3>');
    checkMarkup('!!!!a',       '<h4>a</h4>');
    checkMarkup('!!!!!a',      '<h5>a</h5>');
    checkMarkup('!!!!!!a',     '<h6>a</h6>');
    checkMarkup('!!!!!!!a',    '<h6>!a</h6>');
    checkMarkup('a\n!b',       '<p>a !b</p>');
    checkMarkup('a\n\n!b',     '<p>a</p><h1>b</h1>');
    checkMarkup('!a\n!b',      '<h1>a</h1><h1>b</h1>');
    checkMarkup('!a\n!!b',     '<h1>a</h1><h2>b</h2>');
    checkMarkup('!a\n\n!b',    '<h1>a</h1><h1>b</h1>');
    checkMarkup('!a\nb\n!c',   '<h1>a</h1><p>b !c</p>');
    checkMarkup('!a\nb\n\n!c', '<h1>a</h1><p>b</p><h1>c</h1>');
    checkMarkup('!{}',         '<h1></h1>');
    checkMarkup('!{}.a',       '<h1><span class="wv-markup-a"></span></h1>');
    checkMarkup('!a{}.b',      '<h1>a<span class="wv-markup-b"></span></h1>');
    checkMarkup('!a{b}.c',     '<h1>a<span class="wv-markup-c">b</span></h1>');
    checkMarkup('!a{b}.c ',    '<h1>a<span class="wv-markup-c">b</span> </h1>');
  });
  run("Markup", "markup-list", function () {
    checkMarkup('*',           '<p>*</p>');
    checkMarkup('*\n',         '<p>*</p>');
    checkMarkup('**\n',        '<p>**</p>');
    checkMarkup('*\na',        '<p>* a</p>');
    checkMarkup('#',           '<p>#</p>');
    checkMarkup('#\n',         '<p>#</p>');
    checkMarkup('##\n',        '<p>##</p>');
    checkMarkup('#\na',        '<p># a</p>');
    checkMarkup(' *',          '<p> *</p>');
    checkMarkup(' #',          '<p> #</p>');
    checkMarkup('* ',          '<ul><li> </li></ul>');
    checkMarkup('# ',          '<ol><li> </li></ol>');
    checkMarkup('*a',          '<ul><li>a</li></ul>');
    checkMarkup('**a',         '<ul><li><ul><li>a</li></ul></li></ul>');
    checkMarkup('#a',          '<ol><li>a</li></ol>');
    checkMarkup('##a',         '<ol><li><ol><li>a</li></ol></li></ol>');
    checkMarkup('*#a',         '<ul><li>#a</li></ul>');
    checkMarkup('#*a',         '<ol><li>*a</li></ol>');
    checkMarkup('a\n*b',       '<p>a *b</p>');
    checkMarkup('a\n\n*b',     '<p>a</p><ul><li>b</li></ul>');
    checkMarkup('*a\nb',       '<ul><li>a</li></ul><p>b</p>');
    checkMarkup('*a\n*b',      '<ul><li>a</li><li>b</li></ul>');
    checkMarkup('*a\n\n*b',    '<ul><li>a</li></ul><ul><li>b</li></ul>');
    checkMarkup('*a\n**b',     '<ul><li>a<ul><li>b</li></ul></li></ul>');
    checkMarkup('**a\n**b',    '<ul><li><ul><li>a</li><li>b</li></ul></li></ul>');
    checkMarkup('*a\n#b',      '<ul><li>a</li></ul><ol><li>b</li></ol>');
    checkMarkup('*a\n##b',     '<ul><li>a<ol><li>b</li></ol></li></ul>');
    checkMarkup('**a\n*b',     '<ul><li><ul><li>a</li></ul></li><li>b</li></ul>');
    checkMarkup('*a\n**b\n*c', '<ul><li>a<ul><li>b</li></ul></li><li>c</li></ul>');
    checkMarkup('*a\n##b\n*c', '<ul><li>a<ol><li>b</li></ol></li><li>c</li></ul>');
    checkMarkup('*{}',         '<ul><li></li></ul>');
    checkMarkup('#{}.a',       '<ol><li><span class="wv-markup-a"></span></li></ol>');
    checkMarkup('#a{}.b',      '<ol><li>a<span class="wv-markup-b"></span></li></ol>');
    checkMarkup('#a{b}.c',     '<ol><li>a<span class="wv-markup-c">b</span></li></ol>');
    checkMarkup('#a{b}.c ',    '<ol><li>a<span class="wv-markup-c">b</span> </li></ol>');
    checkMarkup('*a\n{}.b',    '<ul><li>a</li></ul><p><span class="wv-markup-b"></span></p>');
    checkMarkup('{}.a\n*b',    '<p><span class="wv-markup-a"></span> *b</p>');
    checkMarkup('{}.a\n\n*b',  '<p><span class="wv-markup-a"></span></p><ul><li>b</li></ul>');
    //checkMarkup('##a\n*b',     '<ul><li><ol><li>a</li></ol></li><li>b</li></ul>'); WRONG
    checkMarkup('!a\n*b',      '<h1>a</h1><ul><li>b</li></ul>');
    checkMarkup('!\n\n*',      '<p>!</p><p>*</p>');
    checkMarkup('!a\n!\n',     '<h1>a</h1><p>!</p>');
  });
  run("Markup", "markup-key-value", function () {
    checkMarkup('$',            '<p>$</p>');
    checkMarkup('$\n',          '<p>$</p>');
    checkMarkup('$\na',         '<p>$ a</p>');
    checkMarkup('$=',           '<p>$=</p>');
    checkMarkup('$=a',          '<p>$=a</p>');
    checkMarkup('$a',           '<p>$a</p>');
    checkMarkup('$a\n',         '<p>$a</p>');
    checkMarkup('$a\nb',        '<p>$a b</p>');
    checkMarkup('$a=',          '<p>$a=</p>');
    checkMarkup('$a=\n',        '<p>$a=</p>');
    checkMarkup('$a=\nb',       '<p>$a= b</p>');
    checkMarkup('${}={}',       '<dl><dt></dt><dd></dd></dl>');
    checkMarkup('$a=b',         '<dl><dt>a</dt><dd>b</dd></dl>');
    checkMarkup('$a=b\n',       '<dl><dt>a</dt><dd>b</dd></dl>');
    checkMarkup('$a=b\n$c=d',   '<dl><dt>a</dt><dd>b</dd><dt>c</dt><dd>d</dd></dl>');
    checkMarkup('$a=b\nc',      '<dl><dt>a</dt><dd>b</dd></dl><p>c</p>');
    checkMarkup('a\n$b=c',      '<p>a $b=c</p>');
    checkMarkup('a\n\n$b=c',    '<p>a</p><dl><dt>b</dt><dd>c</dd></dl>');
    checkMarkup('$a=b\n\n$c=d', '<dl><dt>a</dt><dd>b</dd></dl><dl><dt>c</dt><dd>d</dd></dl>');
    checkMarkup('$a=b\n*\n',    '<dl><dt>a</dt><dd>b</dd></dl><p>*</p>');
    checkMarkup('$a=b\n$',      '<dl><dt>a</dt><dd>b</dd></dl><p>$</p>');
    checkMarkup('$a=b\n$=',     '<dl><dt>a</dt><dd>b</dd></dl><p>$=</p>');
    checkMarkup('$a=b\n$c',     '<dl><dt>a</dt><dd>b</dd></dl><p>$c</p>');
    checkMarkup('$a=b\n$c=',    '<dl><dt>a</dt><dd>b</dd></dl><p>$c=</p>');
    checkMarkup('$a=b\n$\n',    '<dl><dt>a</dt><dd>b</dd></dl><p>$</p>');
    checkMarkup('$a=b\n$=\n',   '<dl><dt>a</dt><dd>b</dd></dl><p>$=</p>');
    checkMarkup('$a=b\n$c\n',   '<dl><dt>a</dt><dd>b</dd></dl><p>$c</p>');
    checkMarkup('$a=b\n$c=\n',  '<dl><dt>a</dt><dd>b</dd></dl><p>$c=</p>');
  });
  function checkMarkup(input, expected) {
    var actual = $W.markup(input);
    var check = actual === expected;
    if (!check) {
      log(escape(input));
      log('actual:' + escape(actual));
      log('expect:' + escape(expected));
    }
    assertTrue(check);
  }
  function escape(str) {
    var out = '';
    for(var i = 0, length = str.length; i < length; i++) {
      var c = str.charAt(i);
      switch(c) {
        case '&': out += '&amp;'; break;
        case '<': out += '&lt;'; break;
        case '>': out += '&gt;'; break;
        case '\n': out += '\\n'; break;
        case '\r': out += '\\r'; break;
        default: out += c;
      };
    }
    return out;
  }
})(WAVE);
