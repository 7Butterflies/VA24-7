using System.Collections.Generic;

namespace VA24_7_Shared.Model
{
    public enum MedicationTypes
    {
        PatientToPhysician,
        PhysicianToPatient,
    }

    public enum MembershipTypes
    {
        PatientToPhysician,
        PhysicianToPatient,
    }

    public class Membership
    {
        public string closedDateTime { get; set; }

        public string createDateTime { get; set; }

        public string id { get; set; }

        public bool isActive { get; set; }

        public List<Prescription> prescriptions { get; set; }

    }

    public class Person
    {
        public string b2CObjectId { get; set; }

        public string city { get; set; }

        public string country { get; set; }

        public List<string> doctorTopatientMemberships { get; set; } = new List<string>();

        public string email { get; set; }

        public string fullName { get; set; }

        public string id { get; set; }

        public List<string> patientToDoctorMemberships { get; set; } = new List<string>();

        public string role { get; set; }

        public string surname { get; set; }

        public IoTDevice device { get; set; }
    }

    public class Prescription
    {
        public string duration { get; set; }

        public string id { get; set; }

        public List<string> intervals { get; set; }

        public MedicationTypes medicationType { get; set; }

        public string name { get; set; }
    }
}