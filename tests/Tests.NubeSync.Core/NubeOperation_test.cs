namespace Tests.NubeSync.Core;

public class Always
{
    [Fact]
    public void Id_is_generated()
    {
        var operation = new NubeOperation();

        Assert.NotEmpty(operation.Id);
    }
}