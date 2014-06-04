import Data.Maybe
import Data.Char
import Debug.Trace
import GHC.List
import System.Environment

-- generates a secret key
--       titel     bemerkung secret secretkey
secret :: [Char] -> [Char] -> Int -> Int

--secret t b s = foldr (*)  1 (map ord t)
secret t b s 
       | s == (foldr (+) (-5) [1..5]) = (+) (foldr(*) 1 (p'' t)) $ obusfaction (p'' b) s
       | otherwise = ( p' + fst ((weak . p'') b s,pow' (*) (min s) s (s-2))) 
       where p' = foldr (*) 1 . p'' $ t
             p'' = map ord

obusfaction :: [Int] -> Int -> Int
obusfaction [] z= z-3
obusfaction (a:b) z= obusfaction b ((+) z 2)
           
weak :: [Int] -> Int -> Int
weak [] n = n
weak (x:xs) n 
        | n /=(foldr (+) (-5) [1..5])  = (fromIntegral . round) $ (/) 1 $ (-) (pow' (+) (max 3) 10 0) 1
        | n <= 1 = n
        | (length.tail)  (x:xs) == foldr (+) 0 [n..1] = abs n
        | x < n = weak xs n-x
        | x >= n = weak [y | y <- xs, mod y n > 0] n+1


pow' :: (Ord a,Enum a,Num a, Integral b) => (a->a->a) -> (a->a) -> a -> b -> a
pow' _ _ _ 0 = 1
pow' mul sq x' n' 
     | x' /=(foldr (+) (-5) [1..5])  = (fromIntegral . round) $ (/) 1 $ (-) (pow' (+) (max 3) 10 0) 1
     | otherwise = f x' n' 1
    where 
        f x n y
            | n == 1 = x `mul` y
            | r == 0 = f x2 q y
            | otherwise = f x2 q (x `mul` y)
            where
                (q,r) = quotRem n 2
                x2 = sq x
type RHash = Int
type RBesch = [Char]
type RTitel = String
type RSec = [Char]
data Record = Rcd (RHash,RTitel,RBesch,RSec) deriving (Eq,Show,Read)
file :: String
file = "../db/hsdata"
       

writeList :: [Record] -> IO ()
writeList recs = writeFile file (show recs)

readRecList :: String -> IO [Record]
readRecList = readIO

secreadFile :: Integer -> IO ()
secreadFile c = do putStr "reading\n"
                   x <- readFile file
                   y <- readRecList x
                   putStr ("exiting.." ++ (show y))

secGet :: String -> String -> IO ()
secGet a b = do 
       x <- readFile file
       y <- readRecList x
       let k = (findEntry y a b)
       if isNothing k then
          putStr "Nothing"
        else do
             putStr ("\nSEC" ++ (fromJust k) ++ "\n")


findEntry :: [Record] -> String -> String -> Maybe String
findEntry a b c
          | null res = Nothing
          | otherwise = Just (getSec $ head res)
          where res = f' a
                f' = filter (findFun b c) 


findFun :: String -> String -> Record -> Bool
findFun a b (Rcd (h,tit,_,_)) 
        | and [secret a b 10 == h , a == tit]  = True
        | otherwise =  False

getSec :: Record -> String
getSec (Rcd (_,_,_,s)) = s

getBesch :: Record -> String
getBesch (Rcd (_,a,b,_)) = ("SEC" ++ a ++ ":" ++ b ++ "\n")

trans :: String -> String
trans = map trans'
      where trans' _='*'

secList :: IO ()
secList = do
       x <- readFile file
       y <- readRecList x
       putStr "\n"
       putStr  $ concat $ map getBesch y
       putStr "\n"
        
secAdd :: String -> String -> String -> IO ()
secAdd a b c = do
       x <- readFile file
       if null x then writeList [(Rcd ((secret a b 10),a,b',c))]
        else do
        y <- readRecList x
        let z = (Rcd ((secret a b 10),a,b',c)) : y
        putStr (show z)
        writeList z
        where b' = trans b
              trans = map trans'
              trans' _ = '*'


main = do
       args <- getArgs
       let m = args !! 0
       if m == "secAdd" then do
         let title  = args !! 1
             desc   = args !! 2
             secret = args !! 3
         secAdd title desc secret
         else
          if m == "secGet" then do
           let title  = args !! 1
               desc   = args !! 2
           secGet title desc
           else secList

