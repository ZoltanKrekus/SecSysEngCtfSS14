package at.ac.tuwien.inso.ss2014.ctf.conn;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.io.UnsupportedEncodingException;
import java.net.Socket;

import org.apache.commons.codec.binary.Base64;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import at.ac.tuwien.inso.ss2014.ctf.dao.Query;

public class RpcHandler implements Runnable {

	private final Logger logger = LoggerFactory.getLogger(this.getClass());

	private final Socket socket;
	private PrintWriter sResponse;
	private boolean running = false;
	private boolean sendingAllowed = true;
	private BufferedReader cInput;
	private String remoteAddress = null;
	private Query query = null;

	public RpcHandler(Socket socket) {
		this.socket = socket;
		this.query = new Query();
	}

	public Socket getSocket() {
		return socket;
	}

	@Override
	public void run() {
		try {
			sResponse = new PrintWriter(socket.getOutputStream(), true);
			cInput = new BufferedReader(new InputStreamReader(
					socket.getInputStream()));
			String inputLine = "";
			String outputLine = "nuqneH ufo inventory pat! login, vay' Data'nISbogh tlha' calculation Daghaj: cha' wa'vatlh 'ej wej.";
			String response = "";

			remoteAddress = this.socket.getInetAddress().getCanonicalHostName()
					+ " (" + this.socket.getInetAddress().getHostAddress()
					+ ")";

			logger.info(
					"{}: Established connection to remote host {} at remote port {}",
					remoteAddress, remoteAddress, this.socket.getPort());

			sResponse.println(new String(encodeBase64(outputLine)));

			// Waiting for Login
			if ((inputLine = cInput.readLine()) != null) {

				inputLine = new String(decodeBase64(inputLine));

				if (inputLine.equalsIgnoreCase("203")
						|| inputLine.equalsIgnoreCase("two hundred and three")
						|| inputLine.equalsIgnoreCase("two hundred three")
						|| inputLine.equalsIgnoreCase("twohundredthree")
						|| inputLine.equalsIgnoreCase("twohundredandthree")
						|| inputLine.equalsIgnoreCase("zweihundertdrei")
						|| inputLine.equalsIgnoreCase("cha' wa'vatlh wej")) {
					response = "tlhIngan ghov SoH pat! nej Duj pong (per Duj pong). chuH \" baq \" bup"
							+ "\n"
							+ "Recognized human encoding format..."
							+ "\n"
							+ "Changing language to english..."
							+ "\n"
							+ "Instructions:"
							+ "\n"
							+ "!getships <key> --> Selects ships with specified key"
							+ "\n"
							+ "!addship <name of ship> <type of ship> <status of ship> <key> --> Stores a ship into the database"
							+ "\n"
							+ "!terminate --> Closes the connection"
							+ "\n"
							+ "Example..."
							+ "\n"
							+ "!getships 123456789 --> retrieves ships stored with key 123456789 - Give it a try ;-)";
					running = true;

				} else {
					// quitting application
					response = "bup";
				}

				response = castResponse(response);
				sResponse.println(response);
				response = "";
			}

			while (running) {
				if ((inputLine = cInput.readLine()) != null) {
					response = castCommand(inputLine);

					if (sendingAllowed) {
						if (response == null || response == "")
							response = "Nothing!";
						response = castResponse(response);
						sResponse.println(response);
					}

					response = "";

				} else
					running = false;
			}

			logger.info("{}: Closing Connection to {}", remoteAddress,
					remoteAddress);
			sResponse.close();
			cInput.close();

			query.closeConnection();

			socket.close();
			logger.info("{}: Closed Connection to {}", remoteAddress,
					remoteAddress);

		} catch (IOException e) {
			logger.error("{}: {}", remoteAddress, e.getMessage());
			running = false;
			sResponse.close();

			try {
				sResponse.close();
				cInput.close();

				query.closeConnection();

				socket.close();
				logger.error(
						"{}: Connection Error to {} occured. All ressources were closed!",
						remoteAddress, remoteAddress);

			} catch (IOException e1) {
				logger.error("{}: Error closing connection to {}",
						remoteAddress, remoteAddress);
			}

		} catch (ClassNotFoundException e) {
			logger.error("{}: {}", remoteAddress, e.getMessage());
		}
	}

	public void exit() throws UnsupportedEncodingException {
		String res;
		this.running = false;

		res = "bup";
		sResponse.println(castResponse(res));
	}

	public void closeConnection() {
		this.running = false;

		query.closeConnection();

		try {
			socket.close();
		} catch (IOException e) {
			logger.error("{}: Error while closing connection to {}",
					remoteAddress, remoteAddress);
		}
	}

	private byte[] decodeBase64(String base64message) {
		return Base64.decodeBase64(base64message);

	}

	private byte[] encodeBase64(String base64message)
			throws UnsupportedEncodingException {
		return Base64.encodeBase64(base64message.getBytes("UTF-8"));
	}

	private String castCommand(String command) throws ClassNotFoundException,
			IOException {
		String[] commandArchive = null;
		int rowId = 0;

		command = new String(decodeBase64(command));
		logger.info("{}: Client sent (decoded): {}", remoteAddress, command);

		command = command.trim();

		commandArchive = command.split(" ", 2);

		if (commandArchive != null) {

			if (command
					.matches("![gG][eE][tT][sS][hH][iI][pP][sS](\\s\\b)(.+)")) {
				commandArchive = command.split(" ", 2);
				if (commandArchive.length == 2) {
					return query.getShips(remoteAddress, commandArchive[1]);
				}

			} else if (command
					.matches("![aA][dD][dD][sS][hH][iI][pP](\\s\\b)(.+)(\\s\\b)(.+)(\\s\\b)(.+)(\\s\\b)(.+)")) {
				commandArchive = command.split(" ", 5);
				rowId = query
						.addShip(remoteAddress, commandArchive[1],
								commandArchive[2], commandArchive[3],
								commandArchive[4]);
				if (rowId != 0) {
					return "Successfully added ship!";

				} else
					return "Error adding ship! Please try again...";

			} else if (command.equalsIgnoreCase("!terminate")) {
				running = false;
				return "Closing connection...";
			}
		}

		return "Wrong command! Are you trying something funny?! ;)";
	}

	private String castResponse(String response)
			throws UnsupportedEncodingException {
		logger.info("{}: Server-Response: {}", remoteAddress, response);
		response = new String(encodeBase64(response));

		return response;
	}
}