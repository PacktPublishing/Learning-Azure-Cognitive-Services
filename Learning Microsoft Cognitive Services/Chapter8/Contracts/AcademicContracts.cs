using System.Runtime.Serialization;

namespace Chapter8.Contracts
{
    #region Interpret

    [DataContract]
    public class InterpretResponse
    {
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public Interpretation[] interpretations { get; set; }
    }

    [DataContract]
    public class Interpretation
    {
        [DataMember]
        public float logprob { get; set; }
        [DataMember]
        public string parse { get; set; }
        [DataMember]
        public Rule[] rules { get; set; }
    }

    [DataContract]
    public class Rule
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public Output output { get; set; }
    }

    [DataContract]
    public class Output
    {
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string value { get; set; }
    }

    #endregion Interpret

    #region Evaluate

    [DataContract]
    public class EvaluateResponse
    {
        [DataMember]
        public string expr { get; set; }
        [DataMember]
        public Entity[] entities { get; set; }
    }

    [DataContract]
    public class Entity
    {
        [DataMember]
        public float logprob { get; set; }
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string Ti { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public string D { get; set; }
        [DataMember]
        public int CC { get; set; }
        [DataMember]
        public AA[] AA { get; set; }
    }

    [DataContract]
    public class AA
    {
        [DataMember]
        public string AuN { get; set; }
    }

    #endregion Evaluate

    #region Calculate Histogram

    public class HistogramResponse
    {
        public string expr { get; set; }
        public int num_entities { get; set; }
        public Histogram[] histograms { get; set; }
    }

    public class Histogram
    {
        public string attribute { get; set; }
        public int distinct_values { get; set; }
        public int total_count { get; set; }
        public HistogramY[] histogram { get; set; }
    }

    public class HistogramY
    {
        public object value { get; set; }
        public float logprob { get; set; }
        public int count { get; set; }
    }

    #endregion Calculate Histogram

}