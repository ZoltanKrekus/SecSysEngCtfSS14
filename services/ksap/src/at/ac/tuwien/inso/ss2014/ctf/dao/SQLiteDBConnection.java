package at.ac.tuwien.inso.ss2014.ctf.dao;

import java.io.IOException;
import java.net.URL;
import java.net.URLDecoder;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.Vector;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sqlite.SQLiteConnectionPoolDataSource;
import biz.source_code.miniConnectionPoolManager.MiniConnectionPoolManager;

public class SQLiteDBConnection {

	private static final Logger logger = LoggerFactory
			.getLogger(SQLiteDBConnection.class);

	private static String dbFile = null;
	private static String dbPath = null;
	private static MiniConnectionPoolManager poolMgrR = null;
	private static MiniConnectionPoolManager poolMgrRW = null;
	private static final String dbName = "ctfWS2014.db";
	private static Vector<Connection> connectionList = new Vector<Connection>();

	private SQLiteDBConnection() {
	}

	public static synchronized Connection getReadOnlyConnection()
			throws SQLException, IOException, ClassNotFoundException {
		Connection tempConnection = null;
		if (poolMgrR == null) {

			if (dbFile == null)
				initializeDatabase();

			if (dbFile != null) {
				if (!dbFile.equals(dbPath + dbName))
					throw new IOException();
			} else
				throw new IOException();

			SQLiteConnectionPoolDataSource dataSource = new SQLiteConnectionPoolDataSource();
			dataSource.setUrl("jdbc:sqlite:" + dbFile);
			org.sqlite.SQLiteConfig config = new org.sqlite.SQLiteConfig();
			config.setReadOnly(true);
			dataSource.setConfig(config);

			// Instantiate the Connection Pool Manager
			// Hint: Because of concurrency problems/efficiency set to 1
			// connection only
			poolMgrR = new MiniConnectionPoolManager(dataSource, 1);
		}

		tempConnection = poolMgrR.getConnection();
		connectionList.add(tempConnection);

		return tempConnection;
	}

	public static synchronized Connection getConnection() throws SQLException,
			IOException, ClassNotFoundException {
		Connection tempConnection;

		if (poolMgrRW == null) {
			if (dbFile == null)
				initializeDatabase();

			if (dbFile != null) {
				if (!dbFile.equals(dbPath + dbName))
					throw new IOException();
			} else
				throw new IOException();

			SQLiteConnectionPoolDataSource dataSource = new SQLiteConnectionPoolDataSource();
			dataSource.setUrl("jdbc:sqlite:" + dbFile);
			org.sqlite.SQLiteConfig config = new org.sqlite.SQLiteConfig();
			config.setReadOnly(false);
			dataSource.setConfig(config);

			// Instantiate the Connection Pool Manager
			// Hint: Because of concurrency problems/efficiency set to 1
			// connection only
			poolMgrRW = new MiniConnectionPoolManager(dataSource, 1);

			// org.sqlite.SQLiteConfig config = new org.sqlite.SQLiteConfig();
			// config.setReadOnly(false);
			//
			// tempConnection = DriverManager.getConnection("jdbc:sqlite:"
			// + dbPath + dbName, config.toProperties());
			// connectionList.add(tempConnection);
		}

		tempConnection = poolMgrRW.getConnection();
		connectionList.add(tempConnection);

		return tempConnection;
	}

	public static void shutdown() throws SQLException {
		synchronized (connectionList) {
			for (Connection x : connectionList) {
				if (!x.isClosed())
					try {
						x.close();
					} catch (SQLException e) {
						logger.info("Error closing DB-Connection");
					}
			}
		}

		if (poolMgrR != null) {
			poolMgrR.dispose();
			poolMgrR = null;
		}

		if (poolMgrRW != null) {
			poolMgrRW.dispose();
			poolMgrRW = null;
		}
	}

	private static synchronized void initializeDatabase() throws IOException,
			ClassNotFoundException, SQLException {

		URL temp = SQLiteDBConnection.class.getResource("./" + dbName);

		// logger.info(temp.getPath());

		System.setProperty("sqlite.purejava", "true");
		Class.forName("org.sqlite.JDBC");

		if (temp != null) {
			dbFile = URLDecoder.decode(temp.getFile(), "UTF-8");

			if (!dbFile.equalsIgnoreCase("")) {
				logger.info(" > Database already exists! Location: {}", dbFile);
				temp = SQLiteDBConnection.class.getResource(".");
				if (temp != null)
					dbPath = URLDecoder.decode(temp.getPath(), "UTF-8");
				else
					throw new IOException();

				checkDatabase();

			} else {
				createDatabase();
				checkDatabase();
			}

		} else {
			createDatabase();
			checkDatabase();
		}
	}

	private static synchronized void createDatabase() throws IOException,
			ClassNotFoundException, SQLException {
		Connection conn;
		Statement st;

		URL temp = SQLiteDBConnection.class.getResource(".");

		if (temp != null) {
			dbPath = URLDecoder.decode(temp.getPath(), "UTF-8");
			if (dbPath.equals(""))
				throw new IOException();
		} else
			throw new IOException();

		logger.info(" > Setting up new database...");

		conn = DriverManager.getConnection("jdbc:sqlite:" + dbPath + dbName);
		temp = SQLiteDBConnection.class.getResource("./" + dbName);

		if (temp != null) {
			dbFile = URLDecoder.decode(temp.getFile(), "UTF-8");
			if (!dbFile.equals(dbPath + dbName))
				throw new IOException();
		} else
			throw new IOException();

		logger.info(" > Successfully set up database! Location: {}", dbFile);

		logger.info(" > Adding system entries to database...");
		st = conn.createStatement();

		// Create table ship
		st.executeUpdate("CREATE TABLE ship ("
				+ "id INTEGER PRIMARY KEY AUTOINCREMENT,"
				+ "name VARCHAR(255) NOT NULL," + "type VARCHAR(255) NOT NULL,"
				+ "status VARCHAR(255) NOT NULL,"
				+ "key VARCHAR(255) NOT NULL);");

		// Insert into ship
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS Amar', 'K''t''inga-class battle cruiser', 'Destroyed', '123456789');");
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS B''Moth', 'K''t''inga-class battle cruiser', 'Destroyed', '123456789');");
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS Bortas', 'D5 class battle cruiser', 'Disabled', '123456789');");
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS Buruk', 'Bird-of-Prey', 'Active', '123456789');");
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS Ch''Tang', 'Bird-of-Prey', 'Active', '123456789');");
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS Drovana', 'Vor''cha-class attack cruiser', 'Disabled', '123456789');");
		st.executeUpdate("INSERT INTO ship (name, type, status, key) VALUES ('IKS Hor''Cha', 'Warship', 'Active', '123456789');");

		st.close();
		conn.close();

		logger.info(" > System entries successfully stored into database!");
	}

	private static synchronized void checkDatabase() throws IOException,
			ClassNotFoundException, SQLException {
		Connection conn;
		Statement st;
		ResultSet rs;

		logger.info(" > Testing database entries...\n");
		conn = DriverManager.getConnection("jdbc:sqlite:" + dbFile);
		st = conn.createStatement();

		// entity test
		logger.info("=========");
		logger.info("ship test");
		logger.info("=========");
		rs = st.executeQuery("select * from ship;");
		while (rs.next()) {
			logger.info("{} | {} | {} | {} | {}", rs.getInt("id"),
					rs.getString("name"), rs.getString("type"),
					rs.getString("status"), rs.getString("key"));
		}
		logger.info(" ");

		st.close();
		conn.close();

		logger.info("Database entries successfully tested!");
	}
}