#include <Wt/Auth/HashFunction>
#include <Wt/Auth/AuthService>
#include <Wt/Auth/PasswordService>
#include <Wt/Auth/PasswordVerifier>

#include "Session.h"

namespace {
	Wt::Auth::AuthService authService;
	Wt::Auth::PasswordService passwordService(authService);
}

class CryptHashFunction : public Wt::Auth::HashFunction {
	public:
		virtual std::string name() const { return "crypt"; }
		virtual std::string compute(const std::string &msg, const std::string &salt) const {
			std::string crypt_salt = std::string("$1$") + salt + std::string("$");
			return { crypt(msg.c_str(), crypt_salt.c_str()) };
		}
		virtual bool verify(const std::string &msg, const std::string &salt, const std::string &hash) const {
			std::string crypt_salt = hash.substr(0, 2);
			if (hash.substr(0, 3) == "$1$") {
				crypt_salt = std::string("$1$") + salt + std::string("$");
			}
			return (hash == crypt(msg.c_str(), crypt_salt.c_str()));
		}
};

Session::Session(const std::string pwFileName) : _users(new UserDatabase(pwFileName)) {
}

Session::~Session() {
	delete _users;
}

void Session::configureAuth() {
	authService.setAuthTokensEnabled(true, "logincookie");
	authService.setEmailVerificationEnabled(false);

	Wt::Auth::PasswordVerifier *verifier = new Wt::Auth::PasswordVerifier();
	verifier->addHashFunction(new CryptHashFunction());

	passwordService.setVerifier(verifier);
	passwordService.setAttemptThrottlingEnabled(false);
}

const Wt::Auth::AuthService& Session::auth() {
	return authService;
}

const Wt::Auth::AbstractPasswordService& Session::passwordAuth() {
	return passwordService;
}
