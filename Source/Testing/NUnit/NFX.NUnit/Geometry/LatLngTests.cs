/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


using NFX.Geometry;



namespace NFX.NUnit.Geometry
{
    [TestFixture]
    public class LatLngTests
    {
       

       [TestCase]
        public void FromDegreeString_ToString()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
                                 
           Console.WriteLine(cleveland);    
           Assert.AreEqual("41°29'13'', -81°38'26''", cleveland.ToString());    
        }




        [TestCase]
        public void FromDecimalString_Distance_CLE_LA()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var losangeles = new LatLng("34.1610243,-117.9465513");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(losangeles);    

           var dist = cleveland.HaversineEarthDistanceKm(losangeles);

           Console.WriteLine(dist);
           Assert.AreEqual(3265, (int)dist);    
        }

        [TestCase]
        public void FromDecimalString_Distance_LA_CLE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var losangeles = new LatLng("34.1610243,-117.9465513");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(losangeles);    

           var dist = losangeles.HaversineEarthDistanceKm(cleveland);

           Console.WriteLine(dist);
           Assert.AreEqual(3265, (int)dist);    
        }


        [TestCase]
        public void FromDegreeString_Distance_CLE_LA()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
           var losangeles = new LatLng("34°9'40'', -117°56'48''");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(losangeles);    

           var dist = cleveland.HaversineEarthDistanceKm(losangeles);

           Console.WriteLine(dist);
           Assert.AreEqual(3265, (int)dist);    
        }


        [TestCase]
        public void FromDecimalString_Distance_CLE_MOSCOW()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var moscow = new LatLng("55.7530361,37.6217305");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(moscow);    

           var dist = cleveland.HaversineEarthDistanceKm(moscow);

           Console.WriteLine(dist);
           Assert.AreEqual(7786, (int)dist);    
        }

        [TestCase]
        public void FromDecimalString_Distance_MOSCOW_CLE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var moscow = new LatLng("55.7530361,37.6217305");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(moscow);    

           var dist = moscow.HaversineEarthDistanceKm(cleveland);

           Console.WriteLine(dist);
           Assert.AreEqual(7786, (int)dist);    
        }

        [TestCase]
        public void FromDegreeString_Distance_CLE_MOSCOW()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
           var moscow = new LatLng("55°45'11'', 37°37'18''");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(moscow);    

           var dist = cleveland.HaversineEarthDistanceKm(moscow);

           Console.WriteLine(dist);
           Assert.AreEqual(7786, (int)dist);    
        }



        [TestCase]
        public void FromDecimalString_Distance_MELBOURNE_CLE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var melbourne = new LatLng("-37.5210205,144.7461265");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(melbourne);    

           var dist = melbourne.HaversineEarthDistanceKm(cleveland);

           Console.WriteLine(dist);
           Assert.AreEqual(16058, (int)dist);    
        }

        [TestCase]
        public void FromDecimalString_Distance_CLE_MELBOURNE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var melbourne = new LatLng("-37.5210205,144.7461265");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(melbourne);    

           var dist = cleveland.HaversineEarthDistanceKm(melbourne);

           Console.WriteLine(dist);
           Assert.AreEqual(16058, (int)dist);    
        }

        [TestCase]
        public void FromDegreeString_Distance_CLE_MELBOURNE()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
           var melbourne = new LatLng("-37°31'16'', 144°44'46''");
                      
           Console.WriteLine(cleveland);    
           Console.WriteLine(melbourne);    

           var dist = cleveland.HaversineEarthDistanceKm(melbourne);

           Console.WriteLine(dist);
           Assert.AreEqual(16058, (int)dist);    
        }


    }



}