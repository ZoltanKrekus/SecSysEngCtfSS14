#ifndef __SESSION_H
#define __SESSION_H

#include <Wt/Auth/Login>

#include "UserDatabase.h"

class Session {
	public:
		Session(const std::string pwFileName);

		Wt::Auth::Login& login() { return _login; }
		Wt::Auth::AbstractUserDatabase& users() { return *_users; }

		virtual ~Session();

		static const Wt::Auth::AuthService& auth();
		static const Wt::Auth::AbstractPasswordService& passwordAuth();

		static void configureAuth();

	private:
		UserDatabase *_users;
		Wt::Auth::Login _login;
};

#endif
