
### Install Docker
```shell
curl -fsSL https://get.Docker.com -o get-Docker.sh
```
```shell
sudo sh get-Docker.sh
```
```shell
sudo usermod -aG docker $USER
```
```shell
newgrp docker
```
```shell
docker run hello-world
```