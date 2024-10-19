using dbs_labs_back.Models.abstractions;

namespace dbs_labs_back.Utils.abstractions ;

    public interface IWordFactory
    {
        int BytesPerWord { get; }
        int BytesPerBlock { get; }
        IWord CreateQ();
        IWord CreateP();
        IWord Create();
        IWord CreateFromBytes(byte[] bytes, int startFrom);
    }