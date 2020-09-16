namespace DoctorbotTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;


    [Serializable]
    public class doggie
    {
        public int dogNumb { get; set; }
        public string dogName { get; set; }
    }

    [Serializable]
    public class med
    {
        public string medicine_name { get; set; }
        public string medicine_cate { get; set; }
        public string medicine_img { get; set; }
        public string medicine_info { get; set; }
    }

    [Serializable]
    public class dogTypeDisease // 180514 수민 
    {
        public string dog_type { get; set; }
        public string disease { get; set; }
        public string dog_img { get; set; }
        public string disease_info { get; set; }
    }

    [Serializable]
    public class dogTypes
    {
        public string d_type { get; set; }
        public string d_img { get; set; }
    }

    [Serializable]
    public class q1
    {
        public int iNum { get; set; }
        public string symNum { get; set; }
        public string specific_1 { get; set; }
    }

    [Serializable]
    public class q2
    {
        public int myNum { get; set; }
        public string symNum { get; set; }
        public string specific_2 { get; set; }
        public string disease { get; set; }
        public string handle { get; set; }
    }


    [Serializable]
    public class q3
    {
        public int iNum { get; set; }
        public string specific_2 { get; set; }
        public string disease { get; set; }
        public string handle { get; set; }
        public string disease_url { get; set; }
    }
    [Serializable]
    public class q4
    {
        public int iNum { get; set; }
        public string disease { get; set; }
        public string handle { get; set; }
        public string disease_url { get; set; }
    }

    [Serializable]
    public class DiseaseData
    {
        public string symptom { get; set; }
        public string disease { get; set; }
        public string specific_1 { get; set; }
        public string specific_2 { get; set; }
        public string handle { get; set; }
    }
}
