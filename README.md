# Amazon S3 Utils 
S3 utils provide simple set of methods to communicate with any object storage, like Google, Yandex, AWS, Selectel etc. to upload or download files/dictionaries with cmd without complete aws cli.


Use:
```cmd
 bidev.s3 upload file <<full file name>> --service_url ex. https://storage.yandexcloud.net>> --access_key <<your access key from object storage>> --secret_key <<your secret key from object storage>> --bucket <<bucket name>> --path <<path in bucket>>
 bidev.s3 upload directory <<full directory name>> --service_url ex. https://storage.yandexcloud.net>> --access_key <<your access key from object storage>> --secret_key <<your secret key from object storage>> --bucket <<bucket name>> --path <<path in bucket>>
 bidev.s3 url file <<file key in bucket>> --service_url ex. https://storage.yandexcloud.net>> --access_key <<your access key from object storage>> --secret_key <<your secret key from object storage>> --bucket <<bucket name>> --duration_seconds <<Duration in sceonds url will be active>>
 ```
