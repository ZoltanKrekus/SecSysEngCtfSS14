package at.ac.tuwien.inso.ss2014.ctf.dao;

import java.io.IOException;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.SQLException;
import java.sql.Statement;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Query {

	private final Logger logger = LoggerFactory.getLogger(this.getClass());
	private Connection getShipConnection;
	private Connection addShipConnection;

	public String getShips(String remoteAddress, String key)
			throws ClassNotFoundException, IOException {
		String result = "Unable to find ships with key '" + key + "'";
		Connection getShipConnection = null;
		ResultSet rs = null;
		ResultSetMetaData metaData = null;
		String rowResult = null;
		int i = 0;
		int j = 1;

		String query = "SELECT * FROM ship WHERE key = '" + key + "'";

		try {
			logger.info("{} - Query: {}", remoteAddress, query);
			getShipConnection = SQLiteDBConnection.getReadOnlyConnection();

			Statement st = getShipConnection.createStatement();
			rs = st.executeQuery(query);

			while (rs.next()) {
				metaData = rs.getMetaData();
				j = 1;

				while (j <= metaData.getColumnCount()) {
					if (j == 1)
						rowResult = rs.getString(j);
					else
						rowResult = rowResult + " | " + rs.getString(j);
					j++;
				}

				if (i == 0)
					result = rowResult;
				else
					result = result + "\n" + rowResult;
				i++;
			}

		} catch (SQLException e) {
			logger.error("{}: {}", remoteAddress, e.getMessage());
			result = e.getMessage();

		} finally {
			if (rs != null)
				try {
					rs.close();
				} catch (SQLException e) {
					logger.error("{}: {}", remoteAddress, e.getMessage());
				}

			if (getShipConnection != null)
				try {
					getShipConnection.close();
				} catch (SQLException e) {
					logger.error("{}: {}", remoteAddress, e.getMessage());
				}
		}

		logger.info(result);

		return result;
	}

	public synchronized int addShip(String remoteAddress, String name,
			String type, String status, String key)
			throws ClassNotFoundException, IOException {
		int affectedRows;
		PreparedStatement ps = null;
		ResultSet generatedKeys = null;
		ResultSetMetaData metaData = null;
		int result = 0;

		String query = "INSERT INTO ship (name, type, status, key) VALUES ("
				+ name + ", " + type + ", " + status + ", " + key + ");";

		try {
			logger.info("{} - Query: {}", remoteAddress, query);

			addShipConnection = SQLiteDBConnection.getConnection();

			ps = addShipConnection
					.prepareStatement(
							"INSERT INTO ship (name, type, status, key) VALUES (?, ?, ?, ?);",
							Statement.RETURN_GENERATED_KEYS);
			ps.setString(1, name);
			ps.setString(2, type);
			ps.setString(3, status);
			ps.setString(4, key);
			affectedRows = ps.executeUpdate();

			if (affectedRows == 0) {
				throw new SQLException(
						"Creating ship failed, no rows affected.");
			}

			generatedKeys = ps.getGeneratedKeys();
			metaData = generatedKeys.getMetaData();

			if (generatedKeys.next()) {
				if (metaData.getColumnCount() == 1) {
					result = generatedKeys.getInt(1);
					logger.info("Successfully created new ship with ID {}",
							result);
				} else
					throw new SQLException(
							"Creating ship failed, obtained wrong keyset.");
			} else {
				throw new SQLException(
						"Creating ship failed, no generated key obtained.");
			}

		} catch (SQLException e) {
			logger.error("{}: {}", remoteAddress, e.getMessage());

		} finally {
			if (generatedKeys != null)
				try {
					generatedKeys.close();
				} catch (SQLException e) {
					logger.error("{}: {}", remoteAddress, e.getMessage());
				}

			if (ps != null)
				try {
					ps.close();
				} catch (SQLException e) {
					logger.error("{}: {}", remoteAddress, e.getMessage());
				}

			if (addShipConnection != null)
				try {
					addShipConnection.close();
				} catch (SQLException e) {
					logger.error("{}: {}", remoteAddress, e.getMessage());
				}
		}

		return result;
	}

	public void closeConnection() {
		if (getShipConnection != null) {
			try {
				getShipConnection.close();
			} catch (SQLException e) {
				logger.error("{}", e.getMessage());
			}
		}

		if (addShipConnection != null) {
			try {
				addShipConnection.close();
			} catch (SQLException e) {
				logger.error("{}", e.getMessage());
			}
		}
	}
}