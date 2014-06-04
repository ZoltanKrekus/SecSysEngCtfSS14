#ifndef __USERDATABASE_H
#define __USERDATABASE_H

#include <Wt/Auth/User>
#include <Wt/Auth/AbstractUserDatabase>

class UserDatabase : public Wt::Auth::AbstractUserDatabase {
	public:

		struct UserData {
			UserData() = default;
			UserData(std::string name, std::string pwd_hash, std::string salt, std::string hash_alg = "crypt");
			std::string name;
			std::string pwd_hash;
			std::string salt;
			std::string hash_alg = "crypt";
		};

		UserDatabase(const std::string pwFileName);

		virtual Wt::Auth::User findWithIdentity(const std::string &provider, const Wt::WString &identity) const;
		virtual Wt::Auth::User findWithId(const std::string &id) const;
		virtual void removeIdentity(const Wt::Auth::User&, const std::string&) { }
		virtual void addIdentity(const Wt::Auth::User &/*user*/, const std::string &/*provider*/, const Wt::WString &/*id*/) { }
		virtual Wt::WString identity(const Wt::Auth::User &user, const std::string &provider) const;

		virtual Wt::Auth::PasswordHash password(const Wt::Auth::User &query_user) const;

	private:
		std::map<std::string, UserData> _users;
};

#endif
