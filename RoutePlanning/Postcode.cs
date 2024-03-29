﻿using System.Text.RegularExpressions;

namespace RoutePlanning
{
    //https://github.com/rbec/Postcodes/blob/master/Rbec.Postcodes/Postcode.cs
    //https://github.com/ideal-postcodes/postcode/tree/master
    public struct Postcode : IEquatable<Postcode>
    {
        private readonly int _data;

        private Postcode(int data)
        {
            _data = data;
        }

        #region Parsing

        private static bool IsLetter(char c) => c >= 'A';
        private static bool IsDigit(char c) => c < 'A';

        /// <summary>
        /// Checks that a character is letter and converts it to an integer 0..25 ignoring case.
        /// </summary>
        /// <param name="c">The character to parse.</param>
        /// <param name="isInvalid">If the character is a letter the value passed in is unchanged, otherwise it is set to true.</param>
        /// <returns>0..25 if a letter, otherwise undefined.</returns>
        private static int ParseLetter(char c, ref bool isInvalid)
        {
            if (c >= 'a')
                c = (char)(c - 32);
            isInvalid |= c < 'A' || c > 'Z';
            return c - 'A';
        }

        /// <summary>
        /// Checks that a character is digit and converts it to an integer 0..9.
        /// </summary>
        /// <param name="c">The character to parse</param>
        /// <param name="isInvalid">If the character is a digit the value passed in is unchanged, otherwise it is set to true.</param>
        /// <returns>0..9 if a digit, otherwise undefine.d</returns>
        private static int ParseDigit(char c, ref bool isInvalid)
        {
            isInvalid |= c < '0' || c > '9';
            return c - '0';
        }

        /// <summary>
        /// Enumerate the characters of a string, skipping spaces and maintaining the current index.
        /// </summary>
        /// <param name="s">The string to enumerate</param>
        /// <param name="i">
        /// Receives the index from which to start looking for the next non-space character.
        /// Returns the index after the first non-space character.
        /// </param>
        /// <returns>The next non-space character, or if the end of the string is reached the ASCII NULL character.</returns>
        private static char NextNonSpaceCharacter(string s, ref int i)
        {
            while (i < s.Length)
            {
                if (s[i] != ' ')
                    return s[i++];
                i++;
            }
            return default;
        }

        /// <summary>
        /// Validates and parses a string into a postcode.
        /// </summary>
        /// <param name="s">The string to validate and parse.</param>
        /// <param name="postcode"></param>
        /// <returns>Whether the string was a valid postcode.</returns>
        public static bool TryParse(string s, out Postcode postcode)
        {
            // single forward pass avoiding allocations

            var isInvalid = false; // is updated as each character is parsed to reflect if this is a valid postcode

            postcode = default;
            if (s == null)
                return false;

            var currentIndex = 0; // index of the character to be parsed

            #region 1st character of Postcode Area

            var current = NextNonSpaceCharacter(s, ref currentIndex);
            var data = ParseLetter(current, ref isInvalid);

            #endregion

            current = NextNonSpaceCharacter(s, ref currentIndex);
            data *= 27;
            if (IsLetter(current)) // if NOT a letter the encoding scheme uses 0 (data += 0 can be omitted)
            {
                #region (Optional) 2nd character of Postcode Area

                data += ParseLetter(current, ref isInvalid) + 1;
                current = NextNonSpaceCharacter(s, ref currentIndex);

                #endregion
            }

            #region 1st character of Postcode District

            data *= 10;
            data += ParseDigit(current, ref isInvalid);

            #endregion

            current = NextNonSpaceCharacter(s, ref currentIndex);
            var next = NextNonSpaceCharacter(s, ref currentIndex);

            data *= 37;
            if (IsDigit(next)) // look ahead 1 character to see if the current character is part of the District or Sector
            {
                #region (Optional) 2nd character of Postcode District

                if (IsDigit(current)) // if NOT a letter or digit the encoding scheme uses 0 (data += 0 can be omitted)
                    data += ParseDigit(current, ref isInvalid) + 1;
                else
                    data += ParseLetter(current, ref isInvalid) + 11;

                #endregion

                current = next;
                next = NextNonSpaceCharacter(s, ref currentIndex);
            }

            #region Postcode Sector

            data *= 10;
            data += ParseDigit(current, ref isInvalid);

            #endregion

            #region 1st character of Postcode Unit

            data *= 26;
            data += ParseLetter(next, ref isInvalid);

            #endregion

            #region 2nd character of Postcode Unit

            data *= 26;
            current = NextNonSpaceCharacter(s, ref currentIndex);
            data += ParseLetter(current, ref isInvalid);

            #endregion

            if (isInvalid || NextNonSpaceCharacter(s, ref currentIndex) != default(char)) // make sure there are no superfluous charaters at the end
                return false;

            postcode = new Postcode(data);
            return true;
        }

        public static Postcode Parse(string s)
        {
            if (TryParse(s, out var postcode))
                return postcode;
            if (TryFix(s, out postcode))
            {
                return postcode;
            }

            throw new FormatException($"'{s}' is not a valid postcode");
        }

        #region Fixing Postcode

        private static Regex FIXABLE_REGEX = new Regex(@"^\s*[a-z01]{1,2}[0-9oi][a-z\d]?\s*[0-9oi][a-z01]{2}\s*$", RegexOptions.IgnoreCase);

        public static bool TryFix(string s, out Postcode postcode)
        {
            postcode = default;

            Match match = FIXABLE_REGEX.Match(s);

            if (!match.Success) return false;

            s = s.ToUpper().Trim().Replace(@" ", "");

            int l = s.Length;
            string inward = s.Substring(l - 3, 3);
            string outward = s.Substring(0, l - 3);
            string outwardFix = CoerceOutcode(outward);
            string inwardFix = Coerce("NLL", inward);


            string fixedPostcode = $"{outwardFix} {inwardFix}";
            return TryParse(fixedPostcode, out postcode);
        }

        public static Postcode Fix(string s)
        {
            if (TryFix(s, out var postcode))
            {
                return postcode;
            }
            throw new FormatException($"'{s}' is not a valid postcode or can't be fixed");
        }

        private static Dictionary<string, string> ToLetter = new Dictionary<string, string>
    {
        { "0", "O" },
        { "1", "I" }
    };

        private static Dictionary<string, string> ToNumber = new Dictionary<string, string>
    {
        { "O", "0" },
        { "I", "1" }
    };

        private static string CoerceOutcode(string i)
        {
            if (i.Length == 2) return Coerce("LN", i);
            if (i.Length == 3) return Coerce("L??", i);
            if (i.Length == 4) return Coerce("LLN?", i);
            return i;
        }

        /**
         * Given a pattern of letters, numbers and unknowns represented as a sequence
         * of L, Ns and ? respectively; coerce them into the correct type given a
         * mapping of potentially confused letters
         *
         * @hidden
         *
         * @example coerce("LLN", "0O8") => "OO8"
         */
        private static string Coerce(string pattern, string input)
        {
            List<string> acc = new List<string>();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                char target = pattern[i];
                if (target == 'N') acc.Add(ToNumber.ContainsKey(c.ToString()) ? ToNumber[c.ToString()] : c.ToString());
                if (target == 'L') acc.Add(ToLetter.ContainsKey(c.ToString()) ? ToLetter[c.ToString()] : c.ToString());
                if (target == '?') acc.Add(c.ToString());
            }
            return string.Join("", acc);
        }

        #endregion


        #endregion

        #region Formatting

        private static int DivRem(int a, int b, out int remainder)
        {
            var result = a / b;
            remainder = a - result * b;
            return result;
        }

        private static char DecodeLetter(ref int c)
        {
            c = DivRem(c, 26, out var remainder);
            return (char)(remainder + 'A');
        }

        private static char DecodeLetterOrSpace(ref int c)
        {
            c = DivRem(c, 27, out var remainder);

            if (remainder == 0)
                return ' ';
            return (char)(remainder + 'A' - 1);
        }

        private static char DecodeDigitOrLetterOrSpace(ref int c)
        {
            c = DivRem(c, 37, out var remainder);

            if (remainder == 0)
                return ' ';
            if (remainder <= 10)
                return (char)(remainder + '0' - 1);
            return (char)(remainder + 'A' - 11);
        }

        private static char DecodeDigit(ref int c)
        {
            c = DivRem(c, 10, out var remainder);
            return (char)(remainder + '0');
        }

        /// <summary>
        /// Postcode as a normalised string (all uppercase with single space between District and Sector)
        /// </summary>
        public override string ToString()
        {
            var chars = new char[8];
            var data = _data;
            chars[7] = DecodeLetter(ref data);
            chars[6] = DecodeLetter(ref data);
            chars[5] = DecodeDigit(ref data);
            chars[4] = ' ';
            chars[3] = DecodeDigitOrLetterOrSpace(ref data);

            var i = chars[3] == ' ' ? 3 : 2;

            chars[i--] = DecodeDigit(ref data);
            chars[i] = DecodeLetterOrSpace(ref data);
            if (chars[i] != ' ')
                i--;
            chars[i] = DecodeLetter(ref data);
            return new string(chars, i, 8 - i);
        }

        #endregion

        #region IEquatable<Postcode> implementation

        public bool Equals(Postcode other) =>
            _data == other._data;

        public override bool Equals(object obj) =>
            !(obj is null)
            && obj is Postcode postcode
            && Equals(postcode);

        public override int GetHashCode() => _data;

        #endregion
    }
}
