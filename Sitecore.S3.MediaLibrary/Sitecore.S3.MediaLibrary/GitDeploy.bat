cd %1
call git checkout -b source
call git add . -A
call git commit -m "Automated Commit"
call git checkout -b Debug
call git merge source -X theirs
call git push --set-upstream origin Debug:master
exit /b 0