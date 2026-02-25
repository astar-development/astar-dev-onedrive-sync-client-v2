# Common OneDrive Sync Scenarios

This guide provides real-world examples specific to the AStar.Dev.OneDrive.Sync.Client application.

---

## File Upload with Conflict Detection

```csharp
public class FileSyncService
{
    public async Task<Result<SyncResult, string>> UploadFileWithConflictDetectionAsync(
        FilePath localPath,
        OneDrivePath remotePath,
        CancellationToken ct)
    {
        return await ValidateLocalFile(localPath)
            .ToResult(() => "Local file does not exist or is inaccessible")
            .BindAsync(async file => await CheckRemoteFileExists(remotePath, ct))
            .BindAsync(async remoteInfo => await DetectConflict(file, remoteInfo, ct))
            .BindAsync(async resolution => await HandleConflict(resolution, ct))
            .BindAsync(async resolved => await UploadFile(resolved, ct))
            .MapFailure(ex => $"Upload failed: {ex.Message}");
    }
    
    private Option<LocalFileInfo> ValidateLocalFile(FilePath path)
    {
        if (!File.Exists(path.Value))
            return Option.None<LocalFileInfo>();
        
        return new LocalFileInfo(
            path, 
            new FileInfo(path.Value).Length,
            File.GetLastWriteTimeUtc(path.Value)
        ).ToOption();
    }
    
    private async Task<Result<Option<RemoteFileInfo>, Exception>> CheckRemoteFileExists(
        OneDrivePath remotePath,
        CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var response = await _oneDriveClient.GetItemMetadataAsync(remotePath.Value, ct);
            return response != null
                ? Option.Some(new RemoteFileInfo(remotePath, response.Size, response.LastModified))
                : Option.None<RemoteFileInfo>();
        });
    }
    
    private async Task<Result<ConflictResolution, string>> DetectConflict(
        LocalFileInfo local,
        Option<RemoteFileInfo> remoteOption,
        CancellationToken ct)
    {
        return await remoteOption.Match(
            onSome: async remote =>
            {
                if (remote.LastModified > local.LastModified)
                    return await _conflictResolver.ResolveAsync(local, remote, ct);
                
                return new Result<ConflictResolution, string>.Ok(
                    new ConflictResolution(local, ConflictAction.Overwrite));
            },
            onNone: async () =>
            {
                await Task.CompletedTask;
                return new Result<ConflictResolution, string>.Ok(
                    new ConflictResolution(local, ConflictAction.Upload));
            }
        );
    }
}
```

---

## Batch File Synchronization

```csharp
public class BatchSyncService
{
    public async Task<Result<BatchSyncResult, string>> SyncMultipleFilesAsync(
        List<FilePath> localPaths,
        OneDrivePath remoteFolder,
        CancellationToken ct)
    {
        var results = new List<Result<SyncResult, string>>();
        
        foreach (var localPath in localPaths)
        {
            var remotePath = remoteFolder.Combine(localPath.FileName);
            var result = await _fileSyncService.UploadFileAsync(localPath, remotePath, ct);
            results.Add(result);
            
            if (ct.IsCancellationRequested)
                break;
        }
        
        return AggregateSyncResults(results);
    }
    
    private Result<BatchSyncResult, string> AggregateSyncResults(
        List<Result<SyncResult, string>> results)
    {
        var successes = results.Where(r => r is Result<SyncResult, string>.Ok).Count();
        var failures = results.Where(r => r is Result<SyncResult, string>.Error).Count();
        
        var failureMessages = results
            .Match(
                onSuccess: _ => null,
                onFailure: error => error
            )
            .Where(msg => msg != null)
            .ToList();
        
        if (failures == 0)
            return new BatchSyncResult(successes, 0, []);
        
        if (successes == 0)
            return $"All {failures} file(s) failed to sync: {string.Join(", ", failureMessages)}";
        
        return new BatchSyncResult(successes, failures, failureMessages);
    }
}
```

---

## Authentication with Token Refresh

```csharp
public class AuthenticationService
{
    public async Task<Result<AccessToken, string>> GetValidAccessTokenAsync(CancellationToken ct)
    {
        return await GetCachedToken()
            .ToResult(() => "No cached token available")
            .BindAsync(async token => await ValidateToken(token, ct))
            .OrElseAsync(async () => await RefreshTokenAsync(ct))
            .OrElseAsync(async () => await PromptUserLoginAsync(ct))
            .MapFailure(_ => "Authentication failed. Please sign in.");
    }
    
    private Option<AccessToken> GetCachedToken()
    {
        var token = _tokenCache.Get("access_token");
        return token.ToOption();
    }
    
    private async Task<Result<AccessToken, string>> ValidateToken(
        AccessToken token,
        CancellationToken ct)
    {
        if (token.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
            return token;
        
        return await Try.RunAsync(async () =>
        {
            var isValid = await _oneDriveClient.ValidateTokenAsync(token.Value, ct);
            return isValid ? token : throw new InvalidOperationException("Token expired");
        }).MapFailure(_ => "Token validation failed");
    }
    
    private async Task<Result<AccessToken, string>> RefreshTokenAsync(CancellationToken ct)
    {
        var refreshToken = _tokenCache.Get("refresh_token");
        
        return await refreshToken.ToOption()
            .ToResult(() => "No refresh token available")
            .BindAsync(async rt => await Try.RunAsync(async () =>
            {
                var newToken = await _oneDriveClient.RefreshTokenAsync(rt, ct);
                _tokenCache.Set("access_token", newToken);
                return newToken;
            }))
            .MapFailure(ex => $"Token refresh failed: {ex.Message}");
    }
}
```

---

## Download with Progress Tracking

```csharp
public class DownloadService
{
    public async Task<Result<LocalFilePath, string>> DownloadFileAsync(
        OneDrivePath remotePath,
        DirectoryPath localDirectory,
        IProgress<DownloadProgress> progress,
        CancellationToken ct)
    {
        return await ValidateRemoteFile(remotePath, ct)
            .BindAsync(async fileInfo => await EnsureLocalDirectoryExists(localDirectory))
            .BindAsync(async dir => await DownloadWithProgress(remotePath, dir, fileInfo, progress, ct))
            .BindAsync(async tempPath => await VerifyDownload(tempPath, fileInfo, ct))
            .MapFailure(ex => $"Download failed: {ex.Message}");
    }
    
    private async Task<Result<RemoteFileInfo, Exception>> ValidateRemoteFile(
        OneDrivePath remotePath,
        CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var metadata = await _oneDriveClient.GetItemMetadataAsync(remotePath.Value, ct);
            if (metadata == null)
                throw new FileNotFoundException($"Remote file not found: {remotePath.Value}");
            
            return new RemoteFileInfo(remotePath, metadata.Size, metadata.Checksum);
        });
    }
    
    private async Task<Result<DirectoryPath, string>> EnsureLocalDirectoryExists(
        DirectoryPath path)
    {
        return await Try.RunAsync(async () =>
        {
            await Task.Run(() => Directory.CreateDirectory(path.Value));
            return path;
        }).MapFailure(ex => $"Failed to create directory: {ex.Message}");
    }
    
    private async Task<Result<LocalFilePath, Exception>> DownloadWithProgress(
        OneDrivePath remotePath,
        DirectoryPath localDir,
        RemoteFileInfo fileInfo,
        IProgress<DownloadProgress> progress,
        CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var localPath = localDir.Combine(remotePath.FileName);
            
            await using var stream = await _oneDriveClient.DownloadFileStreamAsync(remotePath.Value, ct);
            await using var fileStream = File.Create(localPath.Value);
            
            var buffer = new byte[81920]; // 80KB buffer
            long totalBytesRead = 0;
            int bytesRead;
            
            while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                totalBytesRead += bytesRead;
                
                progress?.Report(new DownloadProgress(
                    totalBytesRead,
                    fileInfo.Size,
                    (double)totalBytesRead / fileInfo.Size * 100
                ));
            }
            
            return localPath;
        });
    }
}
```

---

## Folder Structure Synchronization

```csharp
public class FolderSyncService
{
    public async Task<Result<FolderSyncResult, string>> SyncFolderAsync(
        DirectoryPath localFolder,
        OneDrivePath remoteFolder,
        SyncDirection direction,
        CancellationToken ct)
    {
        return await ValidateLocalFolder(localFolder)
            .ToResult(() => "Local folder does not exist")
            .BindAsync(async _ => await GetFolderStructure(localFolder, remoteFolder, ct))
            .BindAsync(async structure => await DetermineChanges(structure, direction, ct))
            .BindAsync(async changes => await ApplyChanges(changes, ct))
            .MapFailure(ex => $"Folder sync failed: {ex.Message}");
    }
    
    private Option<DirectoryInfo> ValidateLocalFolder(DirectoryPath path)
    {
        var dirInfo = new DirectoryInfo(path.Value);
        return dirInfo.Exists ? Option.Some(dirInfo) : Option.None<DirectoryInfo>();
    }
    
    private async Task<Result<FolderStructure, Exception>> GetFolderStructure(
        DirectoryPath localFolder,
        OneDrivePath remoteFolder,
        CancellationToken ct)
    {
        var localResult = await GetLocalStructure(localFolder, ct);
        var remoteResult = await GetRemoteStructure(remoteFolder, ct);
        
        return localResult.Bind(local =>
            remoteResult.Map(remote =>
                new FolderStructure(local, remote)
            )
        );
    }
    
    private async Task<Result<List<FileSync>, Exception>> DetermineChanges(
        FolderStructure structure,
        SyncDirection direction,
        CancellationToken ct)
    {
        return await Try.RunAsync(async () =>
        {
            var changes = new List<FileSync>();
            
            // Files only in local
            var localOnly = structure.LocalFiles.Except(structure.RemoteFiles, new FilePathComparer());
            if (direction.HasFlag(SyncDirection.Upload))
            {
                changes.AddRange(localOnly.Select(f => new FileSync(f, SyncAction.Upload)));
            }
            
            // Files only in remote
            var remoteOnly = structure.RemoteFiles.Except(structure.LocalFiles, new FilePathComparer());
            if (direction.HasFlag(SyncDirection.Download))
            {
                changes.AddRange(remoteOnly.Select(f => new FileSync(f, SyncAction.Download)));
            }
            
            // Files in both - check timestamps
            var inBoth = structure.LocalFiles.Intersect(structure.RemoteFiles, new FilePathComparer());
            foreach (var file in inBoth)
            {
                var local = structure.LocalFiles.First(f => f.Path == file.Path);
                var remote = structure.RemoteFiles.First(f => f.Path == file.Path);
                
                if (local.LastModified > remote.LastModified && direction.HasFlag(SyncDirection.Upload))
                    changes.Add(new FileSync(local, SyncAction.Upload));
                else if (remote.LastModified > local.LastModified && direction.HasFlag(SyncDirection.Download))
                    changes.Add(new FileSync(remote, SyncAction.Download));
            }
            
            await Task.CompletedTask;
            return changes;
        });
    }
}
```

---

## Settings Management with Validation

```csharp
public class SettingsService
{
    public async Task<Result<Unit, string>> SaveSettingsAsync(
        UserSettings settings,
        CancellationToken ct)
    {
        return await ValidateSettings(settings)
            .BindAsync(async validated => await SerializeSettings(validated))
            .BindAsync(async json => await WriteSettingsFile(json, ct))
            .Tap(_ => _logger.LogInformation("Settings saved successfully"))
            .TapError(error => _logger.LogWarning("Failed to save settings: {Error}", error));
    }
    
    private Result<UserSettings, string> ValidateSettings(UserSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.UserName))
            return "User name is required";
        
        if (!Enum.IsDefined(typeof(ThemeType), settings.SelectedTheme))
            return $"Invalid theme: {settings.SelectedTheme}";
        
        if (settings.SyncIntervalMinutes < 1 || settings.SyncIntervalMinutes > 1440)
            return "Sync interval must be between 1 and 1440 minutes";
        
        return settings;
    }
    
    private Result<string, Exception> SerializeSettings(UserSettings settings)
    {
        return Try.Run(() => JsonSerializer.Serialize(settings, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        }));
    }
    
    public async Task<Result<UserSettings, string>> LoadSettingsAsync(CancellationToken ct)
    {
        return await ReadSettingsFile(ct)
            .BindAsync(json => DeserializeSettings(json))
            .BindAsync(settings => ValidateSettings(settings))
            .OrElse(() => GetDefaultSettings())
            .Tap(settings => _logger.LogInformation("Settings loaded successfully"));
    }
    
    private async Task<Result<string, Exception>> ReadSettingsFile(CancellationToken ct)
    {
        var filePath = GetSettingsFilePath();
        
        return await Try.RunAsync(async () =>
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Settings file not found");
            
            return await File.ReadAllTextAsync(filePath, ct);
        });
    }
    
    private Result<UserSettings, Exception> DeserializeSettings(string json)
    {
        return Try.Run(() => JsonSerializer.Deserialize<UserSettings>(json)
            ?? throw new InvalidOperationException("Deserialized settings is null"));
    }
    
    private Result<UserSettings, string> GetDefaultSettings()
    {
        return new UserSettings
        {
            UserName = "User",
            SelectedTheme = ThemeType.Light,
            SelectedLanguage = "en-GB",
            SyncIntervalMinutes = 30
        };
    }
}
```

---

## See Also

- **Quick Reference**: [`/docs/copilot/quick-reference.md`](/docs/copilot/quick-reference.md)
- **Functional Programming Guide**: [`/docs/copilot/functional-programming-guide.md`](/docs/copilot/functional-programming-guide.md)
- **Migration Guide**: [`/docs/copilot/migration-guide.md`](/docs/copilot/migration-guide.md)
