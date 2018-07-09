/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
package dcemv.cardreaderemulator;

import org.apache.logging.log4j.Level;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.message.Message;
import org.apache.logging.log4j.message.SimpleMessage;
import org.apache.logging.log4j.spi.ExtendedLogger;
import org.apache.logging.log4j.spi.ExtendedLoggerWrapper;

import java.io.IOException;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.SocketException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.function.Consumer;


class Logger{
    private ExtendedLoggerWrapper log;
    static final String FQCN = Logger.class.getName();

    public Logger(Class<?> clazz){
        org.apache.logging.log4j.Logger logger = LogManager.getLogger(clazz);
        log = new ExtendedLoggerWrapper((ExtendedLogger) logger,logger.getName(),logger.getMessageFactory());
    }

    public void info(String s) {
        logEx(Level.INFO,s,null);
    }

    public void error(String s) {
        logEx(Level.ERROR, s,null);
    }

    public void warn(String s) {
        logEx(Level.WARN, s,null);
    }

    public void debug(String s) {
        logEx(Level.DEBUG, s,null);
    }

    public void warn(String s, IOException e) {
        logEx(Level.WARN,s,e);
    }

    private void logEx(Level level, Object message, Throwable t){
        log.logIfEnabled(FQCN,level,null,(Message)(new SimpleMessage(message.toString())), t);
    }
}
class TechnicalException extends RuntimeException{

    public TechnicalException(String s) {
        super(s);
    }

    public TechnicalException(String s, IOException e) {
        super(s,e);
    }
}

public class SocketAcceptor implements AutoCloseable{

    private final static Logger LOGGER = new Logger(SocketAcceptor.class);

    private ServerSocket serverSocket = null;

    private final ExecutorService executorService;
    private final ExecutorService newConnectionService;

    private final String bindIp;
    private final int port;
    private final int socketTimeoutMs;
    private final Consumer<Socket> socketConnectedCallback;

    private Future<?> incomingConnectionService = null;

    public SocketAcceptor(ExecutorService executorService,
                          ExecutorService newConnectionService,
                          String bindIp,
                          int port,
                          int socketTimeoutMs,
                          Consumer<Socket> socketConnectedCallback) {
        this.executorService = executorService;
        this.newConnectionService = newConnectionService;

        this.bindIp = bindIp;
        this.port = port;
        this.socketTimeoutMs = socketTimeoutMs;
        this.socketConnectedCallback = socketConnectedCallback;
    }

    /**
     * starts the appserver and starts accepting connections
     */
    public void start() {
        if(!isRunning()) {
            try {
                serverSocket = new ServerSocket(
                        port,
                        50,     //default if not specified
                        InetAddress.getByName(bindIp));
                serverSocket.setReuseAddress(true);

                LOGGER.info("Starting socketacceptor thread" + bindIp + ":" + port);
                //start a thread to accept connections
                incomingConnectionService = executorService.submit(() -> waitForIncomingConnections(newConnectionService, serverSocket));

            } catch (IOException e) {
                LOGGER.error("Unable to bind to " + bindIp + ":" + port);
                throw new TechnicalException("Unable to bind to " + bindIp + ":" + port, e);
            }
        } else {
            throw new TechnicalException("Socket acceptor is already running.");
        }
    }

    /**
     * stops the appserver and stops accepting connections, but doesn't kill any child threads
     */
    public void stop() {
        if(isRunning()) {
            LOGGER.info("Stopping socket acceptor " + serverSocket);

            try {
                if(!serverSocket.isClosed()) {
                    serverSocket.close();
                }
            } catch (IOException e) {
                LOGGER.warn("Unable to close socket acceptor socket: " + serverSocket);
            }
            serverSocket = null;

            incomingConnectionService.cancel(true);
            try {
                executorService.awaitTermination(5, TimeUnit.SECONDS);
            } catch (InterruptedException e) {
                LOGGER.warn("Not all services have stopped.");
            }
            try {
                newConnectionService.awaitTermination(5, TimeUnit.SECONDS);
            } catch (InterruptedException e) {
                LOGGER.warn("Not all services have stopped.");
            }
            incomingConnectionService = null;
        } else {
            throw new TechnicalException("Socket acceptor is not running.");
        }
    }

    /**
     * function that will wait for incoming connections
     * then spawn a thread to service that connection
     * @param service
     * @param serverSocket
     */
    private void waitForIncomingConnections(ExecutorService service, ServerSocket serverSocket) {
        while (serverSocket != null && !serverSocket.isClosed()) {
            try {
                LOGGER.info("Waiting for a connection on: " + serverSocket.toString());
                // for now just wait for the 1st socket to connect
                final Socket socket = serverSocket.accept();
                socket.setKeepAlive(true);
                socket.setSoTimeout(socketTimeoutMs);

                // spawn a thread to handle the connection
                service.submit(() -> {
                    LOGGER.info("Accepting connection from: " + socket.getInetAddress());
                    socketConnectedCallback.andThen((s) -> LOGGER.info("Connection accepted.")).accept(socket);
                });
            } catch (SocketException se) {
                LOGGER.warn("Accept socket error. shutting it down.");
                return;
            } catch (IOException e) {
                LOGGER.warn("Error waiting for socket, retrying", e);
                Thread.yield(); //backoff
            }
        }
    }

    @Override
    public void close() {
        // stop the socket acceptor thread
        if(isRunning()) {
            stop();
        }
    }

    public boolean isRunning() {
        return serverSocket != null;
    }
}

