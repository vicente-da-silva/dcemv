# *************************************************************************
# DC EMV
# Open Source EMV
# Copyright (C) 2018  Vicente Da Silva
# 
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# any later version.
# 
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
# 
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see http://www.gnu.org/licenses/
# *************************************************************************

# OpenSSL 1.1.0f  25 May 2017

# for cert to work on android make sure subject alt name is set to DNS:www.yourdomain.com as this is what 
# is used to verify cert on Android
# make sure openssl_192.cfg has the correct domain configured in all sections
# e.g. by IP: subjectAltName=IP:192.168.0.100,DNS:192.168.0.100
# e.g. by Domain: subjectAltName=DNS:www.payloola.com

C:\OpenSSL-Win64\bin\openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout server4.key -out server4.crt -config openssl_192.cfg  -subj "/C=SA/ST=GP/O=DCEMVDemoServer/CN=192.168.0.100"

#C:\OpenSSL-Win64\bin\openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout payloola5.key -out payloola5.crt -config openssl_192.cfg  -subj "/C=SA/ST=GP/O=DCEMVDemoServer/CN=www.payloola.com"


