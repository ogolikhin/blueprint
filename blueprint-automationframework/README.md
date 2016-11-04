# blueprint-automationframework
repository for blueprint automation framework

### branch information
```
master: trunk
```

### Getting sync to work

##### Cloning blueprint-automationframework
```
1. Go to the Blueprint repository page that you wish to clone. A list of all available repos can be viewed at:
  - https://github.com/BlueprintSys
2. Select blueprint-automationframework Private as the project of the choice.
3. On the right side of the page you’ll see a link that you can use for cloning. Select SSH and copy the link. Example would look something like git@github.com:BlueprintSys/blueprint-automationframework.git .
4. Create a local directory where you would like to clone this repo.
5. Launch Git Bash and change to this new directory.
6. Type the following command to clone your repo. (Use the link from step 2 above):
  - git clone git@github.com:BlueprintSys/blueprint-current.git
```

##### Getting ready for your first commit
```
In the following example you will make a local change to a branched version of master before merging/pulling it back up to Origin master on github.com.

1. Since you have already cloned your repo above, checkout the branch you want to commit changes to:
  - git checkout master
2. For getting latest changes from master, please run:
  - git pull origin master
3. Branch your local copy of master so that you can start your masterment:
  - git branch dev-123456-mybranch
4. Checkout the new branch:
  - git checkout dev-123456-mybranch
5. Make a change to a file and check the status of your branch:
  - git status  
	-> This returns a message that you have unstaged changes
6. Stage the changed file by either using file name or "-A" (adding all changed files) :
  - git add <filename>
  OR
  - git add -A
7. Run "git status" again to confirmed that you have staged changes ready to be committed.
8. Commit your changes to the local branch:
  - git commit –m “my first commit” 
9. Run "git status" to confirm that you have neither changes ready to commit nor files that remain unstaged.
10. Confirm that your local log contains this commit. You will also see the entire log history for your branch prior to the point where it was branched:
  - git log
11. If the changes ARE NOT large enough to warrant a pull request:
11-1. Switch to your local master branch to merge your changes into:
  - git checkout master
11-2. Merge your changes from mybranch into master:
  - git merge dev-123456-mybranch
11-3. Now you are ready to push it up to origin master:
  - git push origin master
11-4. Go to Github.com to confirm that your commit is there. Congratulations on your first BP commit!
12. If the changes ARE large enough to warrant a pull request:
12-1. Push your local branch to remote.
  - git push origin dev-123456-mybranch
12-2. Create a pull request in Github; @name to message code reviewers.
12-3. Merge pull request in Github after code review is complete.
```
