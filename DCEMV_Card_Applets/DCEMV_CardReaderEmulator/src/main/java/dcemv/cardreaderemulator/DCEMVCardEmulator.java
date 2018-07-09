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

import com.licel.jcardsim.smartcardio.CardSimulator;
import com.licel.jcardsim.utils.AIDUtil;
import dcemv.emvcard.EMVCard;
import dcemv.gpsim.GlobalPlatform;
import dcemv.pse.PPSE;
import javacard.framework.AID;

import javax.smartcardio.CommandAPDU;
import javax.smartcardio.ResponseAPDU;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.math.BigInteger;
import java.net.Socket;
import java.net.SocketTimeoutException;
import java.util.concurrent.SynchronousQueue;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

public class DCEMVCardEmulator {

    private final static Logger LOGGER = new Logger(SocketAcceptor.class);

    private SocketAcceptor sa;
    private CardSimulator simulator;
    private boolean stop;

    private InputStream inputStream;
    private Socket socket;


    public static void main(String[] args) throws Exception {
        DCEMVCardEmulator ece = new DCEMVCardEmulator();
        ece.startEmulator();
        ece.startServer("",50000);
    }

    private void startEmulator() {
        simulator = new CardSimulator();

        AID appletAID = AIDUtil.create("325041592E5359532E4444463031"); //PPSE
        simulator.installApplet(appletAID, PPSE.class);

        appletAID = AIDUtil.create("A000000050010101"); //VISA
        simulator.installApplet(appletAID, EMVCard.class);

        appletAID = AIDUtil.create("A000000018434D00"); //GP
        simulator.installApplet(appletAID, GlobalPlatform.class);

        //simulator.selectApplet(appletAID);
    }

    private void startServer(String ip, int port) {
        ThreadPoolExecutor executorWaitingConnections = new ThreadPoolExecutor(0, Integer.MAX_VALUE, 10, TimeUnit.SECONDS, new SynchronousQueue<Runnable>());
        ThreadPoolExecutor executorConnections = new ThreadPoolExecutor(0, Integer.MAX_VALUE, 10, TimeUnit.SECONDS, new SynchronousQueue<Runnable>());
        sa = new SocketAcceptor(executorWaitingConnections, executorConnections, ip, port, 10000, s -> createConn(s));
        sa.start();
    }

    public static String byteArrayToHexString(byte[] b) {
        String result = "";
        for (int i=0; i < b.length; i++) {
            result +=
                    Integer.toString( ( b[i] & 0xff ) + 0x100, 16).substring( 1 );
        }
        return result;
    }

    private void createConn(Socket s) {
        try {
            if(socket!=null)
                close();

            socket = s;
            inputStream = s.getInputStream();

            while(!stop) {
                byte[] request = readData();

                CommandAPDU commandAPDU = new CommandAPDU(request);
                LOGGER.debug(commandAPDU.toString());
                LOGGER.debug(byteArrayToHexString(commandAPDU.getBytes()));
                ResponseAPDU response = simulator.transmitCommand(commandAPDU);
                LOGGER.debug(response.toString());
                String responseString = byteArrayToHexString(response.getBytes());
                LOGGER.debug(responseString);
                int index = responseString.indexOf("9f36");
                if(index!=-1)
                    LOGGER.debug(responseString.substring(index));

                writeData(response.getBytes());
            }
        } catch (IOException ex) {
            throw new RuntimeException("cannot create inputstream");
        }
    }

    public static int byteArrayToInt(byte[] b) {
        return new BigInteger(b).intValue();
    }

    public static byte[] intToByteArray(int a) {
        return new byte[] {
                //(byte) ((a >> 24) & 0xFF),
                //(byte) ((a >> 16) & 0xFF),
                (byte) ((a >> 8) & 0xFF),
                (byte) (a & 0xFF)
        };
    }

    private void writeData(byte[] data) throws IOException{
        final ByteArrayOutputStream baos = new ByteArrayOutputStream();

        baos.write(intToByteArray(data.length));
        baos.write(data);

        socket.getOutputStream().write(baos.toByteArray());
    }

    private byte[] readData() {
        final ByteArrayOutputStream baos = new ByteArrayOutputStream();

        try {
            byte[] onByteBuffer = new byte[2];
            int bytesRead = inputStream.read(onByteBuffer);
            if(bytesRead!=2)
                throw new RuntimeException("cannot read length");
            int length = byteArrayToInt(onByteBuffer);

            onByteBuffer = new byte[1];
            while(baos.size() < length) {
                bytesRead = inputStream.read(onByteBuffer);
                if (bytesRead == -1) {
                    close();
                    throw new RuntimeException("Unable to read from ethernet");
                }

                baos.write(onByteBuffer, 0, bytesRead);

                if (baos.size() > length) {
                    close();
                    throw new RuntimeException("Max buffer reached.");
                }
            }

            return baos.toByteArray();
        } catch (SocketTimeoutException se) {
            throw new RuntimeException(se);
        } catch (IOException e) {
            try {
                close();
            } catch (IOException e1) {
                // ignore, trying to close
                // close the connection.
            }
            throw new RuntimeException(e);
        }
    }

    private void close() throws IOException{
        inputStream.close();
        socket.close();
    }
}
