using System.Collections.ObjectModel;

namespace EFEntityEntryTesting.EF
{
    internal class DataManager
    {
        private EFTestDataContext _context;

        public ObservableCollection<Quotes> Quotes { get; }
        public ObservableCollection<CategoryList> CategoryList { get; }

        public DataManager()
        {
            _context = new EFTestDataContext();
            _context.Database.EnsureCreated();
            _context.SaveChanges();

            Quotes = _context.Quotes.Local.ToObservableCollection();
            CategoryList = _context.CategoryList.Local.ToObservableCollection();

            _context.Quotes.Add(new(2, "We added data."));
            _context.SaveChanges();

            _context.CategoryList.AddRange(from C in new List<CategoryList>([new("0", "All", 0),
                                                new("156894","FakeId",0),
                                                new("938493","FakeGame",0)])
                                           select C);
            _context.SaveChanges();
        }
    }
}
