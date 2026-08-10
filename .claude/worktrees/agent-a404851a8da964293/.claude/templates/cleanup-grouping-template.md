# Cleanup Grouping Output Shape

**Scope:** used only by `/cleanup`. Defines how the grouping report is presented.

```
## Suggested commit groups

### 1. <type>(<scope>): <suggested subject>
- path/to/file1.cs
- path/to/file2.cs

### 2. <type>(<scope>): <suggested subject>
- path/to/file3.cs
...

## Mixed files (span >1 category — recommend splitting)
- path/to/fileN.cs — contains both <category A> and <category B>; consider `git add -p`

## Suggested order
1. Group N (reason: e.g. "migration must land before the feature that depends on it")
2. Group M
...
```

Never stage or commit anything — this is a report only. If every changed file falls cleanly into one group, the "Mixed files" section is omitted entirely.
