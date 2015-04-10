using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using Pocos.Annotations;

namespace Pocos
{
    public class Contact : INotifyPropertyChanged, IChangeTracking
    {
        private DateTime? _birthday;
        private Movies? _favoriteMovie;
        private bool _enrolledInSpecialProgram;

        [Browsable(false)]
        public int ContactId { get; set; }

        [Browsable(false)]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        [Browsable(false)]
        public string FirstName { get; set; }

        [Browsable(false)]
        public string LastName { get; set; }

        [Browsable(false)]
        public string EmailAddress { get; set; }

        [Browsable(false)]
        public string Phone { get; set; }

        [Display(ShortName = "Birthday", Description = "We will send the contact something related to their favorite movie on their birthday.")]
        [DataField(FieldName = "Birthday")]
        public DateTime? Birthday
        {
            get { return _birthday; }
            set
            {
                if (value.Equals(_birthday)) return;
                _birthday = value;                
                OnPropertyChanged();
            }
        }

        [Display(ShortName = "Favorite Movie", Description = "If the contact's birthday is also specified, we will send them something regarding this choice on their birthday.")]
        [DataField(FieldName = "FavMovie")]
        public Movies? FavoriteMovie
        {
            get { return _favoriteMovie; }
            set
            {
                if (value == _favoriteMovie) return;
                _favoriteMovie = value;                
                OnPropertyChanged();
            }
        }

        [Display(ShortName = "Special Program?", Description = "Indicates whether the customer is enrolled in our special program.")]
        [DataField(FieldName = "SpecialProgram")]
        public bool EnrolledInSpecialProgram
        {
            get { return _enrolledInSpecialProgram; }
            set
            {
                if (value.Equals(_enrolledInSpecialProgram)) return;
                _enrolledInSpecialProgram = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            IsChanged = true;
        }

        public void AcceptChanges()
        {
            IsChanged = false;
        }

        [Browsable(false)]
        public bool IsChanged { get; private set; }
    }

    public enum Movies
    {
        [Description("Star Wars IV: A New Hope")]
        StarWarsNewHope,
        [Description("Guardians of the Galaxy")]
        GuardiansOfGalaxy,
        Jaws,
        Tombstone, 
        [Description("The Usual Suspects")]
        UsualSuspects,
    }
}
