using AktieHandelRepoLib;
using System.Xml.Linq;

namespace AktiehandelRepoLib
{
    /// <summary>
    /// Repository til opbevaring og håndtering af AktieHandel objekter.
    /// </summary>
    public class AktieHandelRepo
    {
        private readonly List<AktieHandel> _aktieHandler = new();

        private int _nextId = 1;

        /// <summary>
        /// Returnerer en aktiehandel med det angivne ID.
        /// </summary>
        /// <param name="id">ID på den ønskede aktiehandel.</param>
        /// <returns>
        /// AktieHandel hvis den findes, ellers null.
        /// </returns>
        public AktieHandel? GetById(int id)
        {
            return _aktieHandler
                .FirstOrDefault(x => x.HandelsId == id);
        }

        /// <summary>
        /// Returnerer alle aktiehandler.
        /// </summary>
        public List<AktieHandel> GetAll()
        {
            return new List<AktieHandel>(_aktieHandler);
        }

        /// <summary>
        /// Returnerer alle aktiehandler.
        /// </summary>
        public List<AktieHandel> Get()
        {
            return new List<AktieHandel>(_aktieHandler);
        }

        /// <summary>
        /// Tilføjer en ny aktiehandel og tildeler automatisk et ID.
        /// </summary>
        /// <param name="aktieHandel">Aktiehandlen der skal tilføjes.</param>
        /// <returns>Den tilføjede aktiehandel.</returns>
        public AktieHandel Add(AktieHandel aktieHandel)
        {
            aktieHandel.Validate();

            aktieHandel.HandelsId = _nextId++;

            _aktieHandler.Add(aktieHandel);

            return aktieHandel;
        }

        /// <summary>
        /// Sletter en aktiehandel med det angivne ID.
        /// </summary>
        /// <param name="id">ID på aktiehandlen.</param>
        /// <returns>
        /// Den slettede aktiehandel eller null hvis den ikke findes.
        /// </returns>
        public AktieHandel? Delete(int id)
        {
            AktieHandel? handel = GetById(id);

            if (handel == null)
            {
                return null;
            }

            _aktieHandler.Remove(handel);

            return handel;
        }

        /// <summary>
        /// Opdaterer en eksisterende aktiehandel.
        /// </summary>
        /// <param name="id">ID på aktiehandlen.</param>
        /// <param name="data">De nye data.</param>
        /// <returns>
        /// Det opdaterede objekt eller null hvis ID ikke findes.
        /// </returns>
        public AktieHandel? Update(int id, AktieHandel data)
        {
            AktieHandel? handel = GetById(id);

            if (handel == null)
            {
                return null;
            }

            data.Validate();

            handel.Navn = data.Navn;
            handel.Antal = data.Antal;
            handel.HandelsPris = data.HandelsPris;

            return handel;
        }

        /// <summary>
        /// Filtrerer aktiehandler efter minimumskurs og eventuelt navn.
        /// </summary>
        /// <param name="kurs">
        /// Kun aktiehandler med HandelsPris større end kurs returneres.
        /// </param>
        /// <param name="navn">
        /// Hvis navn angives, returneres kun handler med dette navn.
        /// </param>
        public List<AktieHandel> Get(decimal kurs, string? navn)
        {
            IEnumerable<AktieHandel> result = _aktieHandler;

            result = result.Where(
                x => x.HandelsPris > kurs);

            if (!string.IsNullOrWhiteSpace(navn))
            {
                result = result.Where(
                    x => x.Navn.Equals(
                        navn,
                        StringComparison.OrdinalIgnoreCase));
            }

            return result.ToList();
        }

        /// <summary>
        /// Filtrerer og sorterer aktiehandler.
        /// </summary>
        /// <param name="kurs">
        /// Kun handler med HandelsPris større end kurs returneres.
        /// </param>
        /// <param name="navn">
        /// Valgfrit navn der kan filtreres på.
        /// </param>
        /// <param name="sortBy">
        /// Sortering: id, id_desc, navn, navn_desc, pris eller pris_desc.
        /// </param>
        public List<AktieHandel> Get(
            decimal kurs,
            string? navn,
            string? sortBy)
        {
            IEnumerable<AktieHandel> result = _aktieHandler;

            result = result.Where(
                x => x.HandelsPris > kurs);

            if (!string.IsNullOrWhiteSpace(navn))
            {
                result = result.Where(
                    x => x.Navn.Equals(
                        navn,
                        StringComparison.OrdinalIgnoreCase));
            }

            switch (sortBy?.ToLower())
            {
                case "id":
                    result = result.OrderBy(
                        x => x.HandelsId);
                    break;

                case "id_desc":
                    result = result.OrderByDescending(
                        x => x.HandelsId);
                    break;

                case "navn":
                    result = result.OrderBy(
                        x => x.Navn);
                    break;

                case "navn_desc":
                    result = result.OrderByDescending(
                        x => x.Navn);
                    break;

                case "pris":
                    result = result.OrderBy(
                        x => x.HandelsPris);
                    break;

                case "pris_desc":
                    result = result.OrderByDescending(
                        x => x.HandelsPris);
                    break;
            }

            return result.ToList();
        }
    }
}