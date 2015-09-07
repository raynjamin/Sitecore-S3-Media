cd %1
call git checkout -b source
call git add . -A
call git commit -m "Automated Commit"
call git push --set-upstream origin source:master
exit /b 0