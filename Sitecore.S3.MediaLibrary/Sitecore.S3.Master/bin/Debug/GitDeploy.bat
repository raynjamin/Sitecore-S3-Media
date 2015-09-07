cd %1
call git add . -A
call git commit -m "Automated Commit"
call git checkout -b source
git push --set-upstream origin source
exit /b 0