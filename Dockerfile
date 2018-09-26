FROM microsoft/dotnet:2.1-aspnetcore-runtime

ADD https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh /bin/wait-for-it.sh
RUN chmod +x /bin/wait-for-it.sh

WORKDIR /app
COPY maplestory.io/run.sh .
RUN chmod +x /app/run.sh
COPY maplestory.io/gms.aes .
COPY maplestory.io/kms.aes .
COPY maplestory.io/build .
ENTRYPOINT ["sh", "run.sh"]
CMD []
