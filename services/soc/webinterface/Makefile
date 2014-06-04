CXXFLAGS = -Wall --std=c++11 -I witty/include
LDFLAGS = -lboost_thread -lboost_system -lboost_program_options -lboost_filesystem -lboost_signals -lboost_random -lboost_regex -lboost_date_time -lssl -lcrypto -lcrypt -lpcrecpp -lz witty/lib/libwthttp.a witty/lib/libwt.a

SRC = main.cc \
	  Session.cc Session.h\
	  SOCApplication.cc SOCApplication.h\
	  UserDatabase.cc UserDatabase.h\
	  MenuWidget.cc MenuWidget.h
OBJ = main.o Session.o SOCApplication.o UserDatabase.o MenuWidget.o

all: soc.wt

soc.wt: ${OBJ}
	${CXX} $^ ${LDFLAGS} -o $@

%.o: %.cc
	${CXX} -c $^ ${CXXFLAGS} -o $@

witty: wt-3.3.2
	mkdir -p wt-build
	cd wt-build; cmake ../wt-3.3.2 -DCMAKE_INSTALL_PREFIX=./witty \
		-DCONFIGDIR=/etc \
		-DCMAKE_BUILD_TYPE=MinSizeRel \
		-DSHARED_LIBS=OFF \
		-DCONNECTOR_FCGI=OFF \
		-DBUILD_EXAMPLES=OFF \
		-DBUILD_TESTS=OFF \
		-DINSTALL_RESOURCES=OFF \
		-DENABLE_HARU=OFF \
		-DENABLE_PANGO=OFF \
		-DENABLE_EXT=OFF \
		-DENABLE_SQLITE=OFF \
		-DENABLE_POSTGRES=OFF \
		-DENABLE_FIREBIRD=OFF \
		-DENABLE_MYSQL=OFF \
		-DENABLE_OPENGL=OFF \
		-DENABLE_QT4=OFF \
		-DCMAKE_CXX_FLAGS="-fvisibility=hidden -fvisibility-inlines-hidden -ffunction-sections -fdata-sections -DHAVE_GNU_REGEX -DWT_NO_SPIRIT -DBOOST_SIGNALS_NO_DEPRECATION_WARNING" -DWEBUSER:STRING=soc \
		-DWEBGROUP:STRING=soc && make install
	ln -s wt-build/witty .

wt-3.3.2: wt-3.3.2.tar.gz
	tar -xzf $<

clean:
	-rm -f *.o *.wt

distclean:
	-rm -rf wt-3.3.2 wt-build witty

start: soc.wt
	./soc.wt --deploy-path=/ --docroot="${PWD}/www;/resources,/script.js,/soc.css,/soc.png,/background.png" --http-address 0.0.0.0 --http-port 2001

