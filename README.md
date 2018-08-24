# Windows File History Cleaner

Simple commandline executable to turn a File History share back into a snapshot of files

## Sample command:

```
FileHistroyCleaner remove-history 
    --root "C:\BACKUP" 
    --recursive 
    --rd "(\\node_modules$|\\\$tf$|\.git$|\\locallow$|\\bin$|\\obj$|packages$)" 
    --rf "(\.DS_Store|.lnk$|\.exe$|\.msi$|\.iso$|\.dll$)"  
    --verbose 
    --force
    --whatif
```

 * `--root` points to the root of a File History share (or copy of one)
 * `--recursive` traverses all sub directories
 * `--rd` deletes directory recursively if it match the supplied regex. Be careful of the escape rules of both your commandline processor and regex.
 * `--rf` deletes files if it match the supplied regex. Be careful of the escape rules of both your commandline processor and regex.
 * `--verbose` outputs more logs
 * `--force` delete instead of move to the recycle bin
 * `--whatif` Output actions, but do not actually perform them
 
 ## History
 
 I wrote this little tool after my NAS became inaccessible. While taking a backup of the drives I found out the File History Snapshot of my laptop had grown beyong 1.4TB. 
 
 By running this little cleanup tool I managed to return the snapshot to 60GB of only the latest versions of every file.
