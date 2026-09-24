using System;
using System.Collections.Generic;
using System.Text;

namespace AktieHandelRepoLib
{
            
        /// <summary>
        /// Repræsenterer en aktiehandel.
        /// Et positivt antal betyder køb.
        /// Et negativt antal betyder salg.
        /// </summary>
        public class AktieHandel
        {
            /// <summary>
            /// Unikt ID for aktiehandlen.
            /// </summary>
            public int HandelsId { get; set; }

            /// <summary>
            /// Navnet på aktien.
            /// Navnet må ikke være null og skal være mindst 4 tegn langt.
            /// </summary>
            public string Navn { get; set; } = "";

            /// <summary>
            /// Antallet af aktier.
            /// Positivt antal betyder køb og negativt antal betyder salg.
            /// </summary>
            public int Antal { get; set; }

            /// <summary>
            /// Prisen aktien blev handlet til.
            /// </summary>
            public decimal HandelsPris { get; set; }

            /// <summary>
            /// Validerer hele aktiehandlen.
            /// </summary>
            public void Validate()
            {
                ValidateNavn();
                ValidateAntal();
                ValidateHandelsPris();
            }

            /// <summary>
            /// Validerer navnet.
            /// </summary>
            /// <exception cref="ArgumentNullException">
            /// Kastes hvis Navn er null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Kastes hvis Navn er kortere end 4 tegn.
            /// </exception>
            public void ValidateNavn()
            {
                if (Navn == null)
                {
                    throw new ArgumentNullException(nameof(Navn));
                }

                if (Navn.Length < 4)
                {
                    throw new ArgumentException(
                        "Navn skal være mindst 4 tegn langt.",
                        nameof(Navn));
                }
            }

            /// <summary>
            /// Validerer antallet.
            /// Antal må ikke være 0.
            /// </summary>
            public void ValidateAntal()
            {
                if (Antal == 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Antal),
                        "Antal må ikke være 0.");
                }
            }

            /// <summary>
            /// Validerer handelsprisen.
            /// </summary>
            public void ValidateHandelsPris()
            {
                if (HandelsPris <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(HandelsPris),
                        "HandelsPris skal være større end 0.");
                }
            }

            /// <summary>
            /// Returnerer aktiehandlen som tekst.
            /// </summary>
            public override string ToString()
            {
                return $"Id: {HandelsId}, Navn: {Navn}, Antal: {Antal}, Pris: {HandelsPris}";
            }
        }
}
