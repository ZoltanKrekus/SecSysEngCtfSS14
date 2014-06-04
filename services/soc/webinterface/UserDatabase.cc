#include "UserDatabase.h"

UserDatabase::UserData::UserData(std::string name, std::string pwd_hash, std::string salt, std::string hash_alg) : name(name), pwd_hash(pwd_hash), salt(salt), hash_alg(hash_alg) { }

UserDatabase::UserData createUser(const std::string& userid, const std::string& pwdhash) {
	std::string salt;
	if (pwdhash.find("$") == 0) {
		auto salt_end = pwdhash.find("$", 3);
		salt = pwdhash.substr(3, salt_end - 3);
	} else {
		// old DES password
		salt = pwdhash.substr(0, 2);
	}

	return { userid, pwdhash, salt, "crypt" };
}

UserDatabase::UserDatabase(const std::string pwFileName) {
	std::ifstream pwFile(pwFileName);

	size_t lineCounter = 0;
	std::string pwLine;
	while (std::getline(pwFile, pwLine)) {
		std::string username, password;
		std::stringstream buffer(pwLine);
		++lineCounter;

		if (buffer.str().length() == 0) {
			std::cerr << "skipping empty line " << lineCounter << std::endl;
			continue;
		}

		if (buffer.peek() == '#') {
			std::cerr << "skipping line " << lineCounter << " '" << buffer.str() << "'" << std::endl;
			continue;
		}

		buffer >> username >> password;

		if (username.empty() || password.empty()) {
			std::cerr << "skipping malformed line " << lineCounter << std::endl;
			continue;
		}

		std::cerr << "adding user " << username << std::endl;
		_users[username] = createUser(username, password);
	}
}

Wt::Auth::User UserDatabase::findWithIdentity(const std::string &, const Wt::WString &identity) const {
	auto user = _users.find(identity.toUTF8());
	if (user == _users.end()) {
		return Wt::Auth::User();
	}
	return { user->first, *this };
}

Wt::Auth::User UserDatabase::findWithId(const std::string &id) const {
	auto user = _users.find(id);
	if (user == _users.end()) {
		return Wt::Auth::User();
	}
	return { user->first, *this };
}

Wt::WString UserDatabase::identity(const Wt::Auth::User &user, const std::string &) const {
	return user.id();
}

Wt::Auth::PasswordHash UserDatabase::password(const Wt::Auth::User &query_user) const {
	auto user = _users.find(query_user.id());
	if (user == _users.end()) {
		std::cerr << "password request for unknown user with id: " << query_user.id() << std::endl;
		throw std::runtime_error("password request for unknown user");
	}
	return { user->second.hash_alg, user->second.salt, user->second.pwd_hash };
}

