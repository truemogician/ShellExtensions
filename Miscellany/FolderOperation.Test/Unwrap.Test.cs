namespace FolderOperation.Test;

using System.IO;

public class UnwrapTests {
    private string Root { get; set; } = null!;

    [SetUp]
    public void Setup() {
        Root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(Root);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(Root))
            Directory.Delete(Root, true);
    }

    // Utility function to create directories more conveniently
    private static string[] CreateDirs(string testRoot, params string[] relativePaths) {
        var fullPaths = new string[relativePaths.Length];
        for (var i = 0; i < relativePaths.Length; i++) {
            fullPaths[i] = Path.Combine(testRoot, relativePaths[i]);
            Directory.CreateDirectory(fullPaths[i]);
        }
        return fullPaths;
    }

    // Utility function to create a file (no content needed)
    private static string CreateFile(string testRoot, string relativePath) {
        var fullPath = Path.Combine(testRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, string.Empty);
        return fullPath;
    }

    [Test]
    public void TestDeleteEmpty() {
        // Create test root folder
        var testRoot = Path.Combine(Root, "TestDeleteEmpty");
        Directory.CreateDirectory(testRoot);

        // Create empty folders and a non-empty folder with a file
        var paths = CreateDirs(testRoot, "EmptyFolder1", "EmptyFolder2", "NonEmptyFolder");
        var filePath = CreateFile(testRoot, "NonEmptyFolder/file.txt");

        // Execute unwrap with DeleteEmpty flag
        var folderOp = new FolderOperation([paths[0], paths[1], paths[2]]);
        folderOp.Unwrap(UnwrapOption.DeleteEmpty);

        // Verify empty folders are deleted but non-empty folder remains
        Assert.Multiple(() => {
            Assert.That(Directory.Exists(paths[0]), Is.False);
            Assert.That(Directory.Exists(paths[1]), Is.False);
            Assert.That(Directory.Exists(paths[2]), Is.True);
            Assert.That(File.Exists(filePath), Is.True);
        });
    }

    [Test]
    public void TestUnwrap() {
        // Create test root folder
        var testRoot = Path.Combine(Root, "TestUnwrap");
        Directory.CreateDirectory(testRoot);

        // Setup folder with a file
        var dir = Path.Combine(testRoot, "FolderToUnwrap");
        var filePath = CreateFile(testRoot, "FolderToUnwrap/file.txt");

        // Also create a folder with multiple files
        var multipleDir = Path.Combine(testRoot, "MultipleFiles");
        var file1 = CreateFile(testRoot, "MultipleFiles/file1.txt");
        var file2 = CreateFile(testRoot, "MultipleFiles/file2.txt");

        // Execute unwrap
        var folderOp = new FolderOperation([dir, multipleDir]);
        folderOp.Unwrap(UnwrapOption.Unwrap);

        // Verify folders are unwrapped (deleted) and files are moved to parent
        var movedFile = Path.Combine(testRoot, "file.txt");
        var movedFile1 = Path.Combine(testRoot, "file1.txt");
        var movedFile2 = Path.Combine(testRoot, "file2.txt");

        Assert.Multiple(() => {
            Assert.That(Directory.Exists(dir), Is.False);
            Assert.That(Directory.Exists(multipleDir), Is.False);
            Assert.That(File.Exists(movedFile), Is.True);
            Assert.That(File.Exists(movedFile1), Is.True);
            Assert.That(File.Exists(movedFile2), Is.True);
        });
    }

    [Test]
    public void TestDeleteEmptyDeep() {
        // Create test root folder
        var testRoot = Path.Combine(Root, "TestDeleteEmptyDeep");
        Directory.CreateDirectory(testRoot);

        // Create nested empty folders
        CreateDirs(testRoot, "Parent", "Parent/Child", "Parent/Child/GrandchildEmpty");

        // Also create a non-empty folder path for verification
        var filePath = CreateFile(testRoot, "NonEmpty/Child/file.txt");

        // Execute delete empty with deep flag
        var parentDir = Path.Combine(testRoot, "Parent");
        var nonEmptyPath = Path.Combine(testRoot, "NonEmpty");
        var folderOp = new FolderOperation([parentDir, nonEmptyPath]);
        folderOp.Unwrap(UnwrapOption.DeleteEmpty | UnwrapOption.Deep);

        // Verify all empty folders in hierarchy are deleted and non-empty ones remain
        Assert.Multiple(() => {
            Assert.That(Directory.Exists(parentDir), Is.False);
            Assert.That(Directory.Exists(nonEmptyPath), Is.True);
            Assert.That(File.Exists(filePath), Is.True);
        });
    }

    [Test]
    public void TestUnwrapDeep() {
        // Create test root folder
        var testRoot = Path.Combine(Root, "TestUnwrapDeep");
        Directory.CreateDirectory(testRoot);

        // Create nested folder structure with file at deepest level
        var filePath = CreateFile(testRoot, "Level1/Level2/Level3/file.txt");

        // Execute unwrap with deep flag
        var level1 = Path.Combine(testRoot, "Level1");
        var folderOp = new FolderOperation([level1]);
        folderOp.Unwrap(UnwrapOption.Unwrap | UnwrapOption.Deep);

        // Verify nested folders are removed and file is moved to root level
        var movedFile = Path.Combine(testRoot, "file.txt");

        Assert.Multiple(() => {
            Assert.That(Directory.Exists(level1), Is.False);
            Assert.That(File.Exists(movedFile), Is.True);
        });
    }

	// Utility function to create deep folder structure with multiple files
	private string CreateDeepFolderStructure(string testName) {
		var testRoot = Path.Combine(Root, testName);
		Directory.CreateDirectory(testRoot);
        CreateFile(testRoot, "Level1/Level2/file1.txt");
		CreateFile(testRoot, "Level1/Level2/file2.txt");
		CreateFile(testRoot, "Level1/Level2/Subfolder/file3.txt");
		return testRoot;
	}

    [Test]
    public void TestUnwrapDeepWithoutMultipleFlag() {
        // Create nested folder structure with multiple files
		var testRoot = CreateDeepFolderStructure(nameof(TestUnwrapDeepWithoutMultipleFlag));
		var level1 = Path.Combine(testRoot, "Level1");

        // Execute unwrap with Deep flag but without UnwrapMultipleDeep
        var folderOp = new FolderOperation([level1]);
        folderOp.Unwrap(UnwrapOption.Unwrap | UnwrapOption.Deep);

        // Verify level2 is moved to root but its contents remain inside
        var level2Moved = Path.Combine(testRoot, "Level2");

        Assert.Multiple(() => {
            Assert.That(Directory.Exists(level1), Is.False);
            Assert.That(Directory.Exists(level2Moved), Is.True);
            Assert.That(File.Exists(Path.Combine(level2Moved, "file1.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(level2Moved, "file2.txt")), Is.True);
        });
    }

    [Test]
    public void TestUnwrapDeepWithMultipleFlag() {
		// Create nested folder structure with multiple files
		var testRoot = CreateDeepFolderStructure(nameof(TestUnwrapDeepWithMultipleFlag));
		var level1 = Path.Combine(testRoot, "Level1");

        // Execute unwrap with Deep and UnwrapMultipleDeep flags
        var folderOp = new FolderOperation([level1]);
        folderOp.Unwrap(UnwrapOption.Unwrap | UnwrapOption.Deep | UnwrapOption.UnwrapMultipleDeep);

        // Verify all files are moved to root and all folders are deleted
        var rootFile1 = Path.Combine(testRoot, "file1.txt");
        var rootFile2 = Path.Combine(testRoot, "file2.txt");
        var rootFile3 = Path.Combine(testRoot, "Subfolder/file3.txt");

        Assert.Multiple(() => {
            Assert.That(Directory.Exists(level1), Is.False);
            Assert.That(Directory.Exists(Path.Combine(testRoot, "Level2")), Is.False);
            Assert.That(File.Exists(rootFile1), Is.True);
            Assert.That(File.Exists(rootFile2), Is.True);
			Assert.That(File.Exists(rootFile3), Is.True);
        });
    }
}